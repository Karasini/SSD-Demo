"""Suppress known harmless warnings from optional ML dependencies."""

import warnings


def configure() -> None:
    # pyannote pulls torchcodec; CPU Docker images lack CUDA libs (e.g. libnvrtc).
    # WhisperX loads audio via ffmpeg / whisperx.load_audio — torchcodec is unused.
    warnings.filterwarnings(
        "ignore",
        message=r".*torchcodec.*",
        category=UserWarning,
        module=r"pyannote\.audio\.core\.io",
    )
    # Common in Docker / QEMU when onnxruntime probes CPU vendor.
    warnings.filterwarnings(
        "ignore",
        message=r".*Unknown CPU vendor.*",
        category=UserWarning,
    )
