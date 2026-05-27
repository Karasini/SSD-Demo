SHELL := /bin/bash

.PHONY: run-api run-worker run-web run-docker-infra run-docker-full run-apps setup clean-docker install-worker

setup:
	@[ -f web/.env ] || cp web/.env.example web/.env
	@[ -f transcriptor-worker/.env ] || cp transcriptor-worker/.env.example transcriptor-worker/.env
	@echo "Setup complete. .env files are ready."

run-api:
	cd api-dotnet/Transcriptor.Api && dotnet run

install-worker:
	cd transcriptor-worker && \
	( [ -d .venv ] || python3 -m venv .venv ) && \
	.venv/bin/pip install -r requirements.txt

run-worker: install-worker
	cd transcriptor-worker && .venv/bin/uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload

run-web:
	cd web && npm run dev

run-apps:
	@trap 'kill 0' SIGINT; \
	(cd api-dotnet/Transcriptor.Api && dotnet run) & \
	(cd transcriptor-worker && .venv/bin/uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload) & \
	(cd web && npm run dev) & \
	wait

run-docker-infra:
	docker compose -f ../infrastructure/docker-compose.yml up -d postgres minio

run-docker-full:
	docker compose -f ../infrastructure/docker-compose.yml up -d --build

clean-docker:
	docker compose -f ../infrastructure/docker-compose.yml down -v

