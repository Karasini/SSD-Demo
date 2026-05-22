"""Suppress known harmless warnings from optional ML dependencies."""

import logging
import os
import warnings


def configure() -> None:
    os.environ.setdefault("HF_HUB_DISABLE_PROGRESS_BARS", "1")
    os.environ.setdefault("TQDM_DISABLE", "1")

    # pyannote (WhisperX VAD) probes torchcodec; CPU Docker lacks CUDA libs (libnvrtc).
    # Audio is decoded via ffmpeg / whisperx.load_audio — torchcodec is never used.
    warnings.filterwarnings("ignore", category=UserWarning, module=r"pyannote\.audio")
    warnings.filterwarnings("ignore", message=r"torchcodec", category=UserWarning)
    warnings.filterwarnings("ignore", message=r"libtorchcodec", category=UserWarning)
    warnings.filterwarnings("ignore", message=r"libnvrtc", category=UserWarning)

    logging.getLogger("pyannote").setLevel(logging.ERROR)
    logging.getLogger("onnxruntime").setLevel(logging.ERROR)

    try:
        from pydantic.warnings import UnsupportedFieldAttributeWarning

        warnings.filterwarnings("ignore", category=UnsupportedFieldAttributeWarning)
    except ImportError:
        pass

    warnings.filterwarnings(
        "ignore", message=r".*Unknown CPU vendor.*", category=UserWarning
    )
