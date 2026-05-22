import logging
import threading
import time
from concurrent.futures import ThreadPoolExecutor
from typing import Any

from app.config import settings

logger = logging.getLogger(__name__)

_model: Any | None = None
_align_models: dict[str, tuple[Any, Any]] = {}
_model_lock = threading.Lock()
_align_lock = threading.Lock()
_ready = threading.Event()


def _load_asr() -> Any:
    import whisperx  # type: ignore

    logger.info(
        "Loading Whisper ASR model %s into memory (device=%s, compute_type=%s; "
        "medium on CPU often takes ~1 min before alignment downloads begin)",
        settings.whisper_model,
        settings.whisper_device,
        settings.whisper_compute_type,
    )
    started = time.monotonic()
    model = whisperx.load_model(
        settings.whisper_model,
        settings.whisper_device,
        compute_type=settings.whisper_compute_type,
    )
    logger.info("Whisper ASR model loaded in %.1fs", time.monotonic() - started)
    return model


def _load_align(language_code: str) -> tuple[Any, Any]:
    import whisperx  # type: ignore

    logger.info(
        "Loading alignment model for language %s (device=%s)",
        language_code,
        settings.whisper_device,
    )
    started = time.monotonic()
    result = whisperx.load_align_model(
        language_code=language_code,
        device=settings.whisper_device,
    )
    logger.info(
        "Alignment model for %s loaded in %.1fs",
        language_code,
        time.monotonic() - started,
    )
    return result


def get() -> Any:
    global _model
    if _model is not None:
        return _model
    with _model_lock:
        if _model is None:
            _model = _load_asr()
        return _model


def get_align(language_code: str) -> tuple[Any, Any]:
    cached = _align_models.get(language_code)
    if cached is not None:
        return cached
    loaded = _load_align(language_code)
    with _align_lock:
        if language_code not in _align_models:
            _align_models[language_code] = loaded
        return _align_models[language_code]


def wait_ready(timeout: float | None = None) -> bool:
    """Block until startup warmup finished. Returns False on timeout."""
    return _ready.wait(timeout=timeout)


def warmup() -> None:
    """Pre-download ASR and alignment models configured for warmup."""
    started = time.monotonic()
    get()

    languages = settings.warmup_language_list
    if len(languages) == 1:
        get_align(languages[0])
    elif languages:
        logger.info("Loading alignment models in parallel: %s", ", ".join(languages))
        with ThreadPoolExecutor(max_workers=len(languages)) as pool:
            list(pool.map(get_align, languages))

    _ready.set()
    logger.info(
        "Whisper models ready in %.1fs (ASR=%s, alignment=%s)",
        time.monotonic() - started,
        settings.whisper_model,
        ", ".join(languages),
    )


def is_ready() -> bool:
    return _ready.is_set()
