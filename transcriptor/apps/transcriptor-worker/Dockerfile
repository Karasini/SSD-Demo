FROM python:3.11-slim

RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY app ./app

ARG WHISPER_MODEL=medium
ARG WHISPER_WARMUP_LANGUAGES=en,pl
ENV WHISPER_DEVICE=cpu
ENV WHISPER_COMPUTE_TYPE=int8
ENV WHISPER_MODEL=${WHISPER_MODEL}
ENV WHISPER_WARMUP_LANGUAGES=${WHISPER_WARMUP_LANGUAGES}
ENV HF_HUB_DISABLE_PROGRESS_BARS=1
ENV TQDM_DISABLE=1

# Bake model weights into the image so runtime warmup only loads into RAM (no HTTP).
RUN python -c "from app.whisper_model import warmup; warmup()"

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
