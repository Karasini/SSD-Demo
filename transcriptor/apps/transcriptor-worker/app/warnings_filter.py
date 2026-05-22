"""Suppress known harmless warnings from optional ML dependencies."""

import os
import warnings


def configure() -> None:
    os.environ.setdefault("HF_HUB_DISABLE_PROGRESS_BARS", "1")
    os.environ.setdefault("TQDM_DISABLE", "1")
    try:
        from pydantic.warnings import UnsupportedFieldAttributeWarning

        warnings.filterwarnings("ignore", category=UnsupportedFieldAttributeWarning)
    except ImportError:
        pass

    # pyannote pulls torchcodec; CPU Docker images lack CUDA libs (e.g. libnvrtc).
    # WhisperX loads audio via ffmpeg / whisperx.load_audio — torchcodec is unused.
    warnings.filterwarnings("ignore", message=r".*torchcodec.*", category=UserWarning)
    warnings.filterwarnings("ignore", message=r".*libtorchcodec.*", category=UserWarning)

    # Common in Docker / QEMU when onnxruntime probes CPU vendor.
    warnings.filterwarnings("ignore", message=r".*Unknown CPU vendor.*", category=UserWarning)
