# Architecture

See the knowledge-base technical specification for the full flow:

- Web → API only (no auth v1)
- API → PostgreSQL + MinIO
- API → Worker (HTTP trigger)
- Worker → API (PATCH callback with internal API key)

Worker runs WhisperX transcription (CPU in local Docker by default).
