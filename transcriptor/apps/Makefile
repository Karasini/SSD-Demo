SHELL := /bin/bash

.PHONY: run-api run-worker run-web run-docker-infra run-docker-full run-apps setup

setup:
	@[ -f web/.env ] || cp web/.env.example web/.env
	@[ -f transcriptor-worker/.env ] || cp transcriptor-worker/.env.example transcriptor-worker/.env
	@echo "Setup complete. .env files are ready."

run-api:
	cd api-dotnet/Transcriptor.Api && dotnet run

run-worker:
	cd transcriptor-worker && uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload

run-web:
	cd web && npm run dev

run-apps:
	@trap 'kill 0' SIGINT; \
	(cd api-dotnet/Transcriptor.Api && dotnet run) & \
	(cd transcriptor-worker && uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload) & \
	(cd web && npm run dev) & \
	wait

run-docker-infra:
	docker compose -f ../infrastructure/docker-compose.yml up -d postgres minio

run-docker-full:
	docker compose -f ../infrastructure/docker-compose.yml up -d --build

