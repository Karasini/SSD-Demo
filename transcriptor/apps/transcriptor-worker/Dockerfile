FROM python:3.11-slim

RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

ARG INSTALL_WHISPERX=false

COPY requirements.txt requirements-whisperx.txt ./
RUN pip install --no-cache-dir -r requirements.txt \
    && if [ "$INSTALL_WHISPERX" = "true" ]; then pip install --no-cache-dir -r requirements-whisperx.txt; fi

COPY app ./app

ENV MOCK_MODE=true
ENV WHISPER_DEVICE=cpu
ENV WHISPER_COMPUTE_TYPE=int8
ENV WHISPER_MODEL=base

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
