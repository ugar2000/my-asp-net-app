.PHONY: backend-up frontend-up app-up

# Default target
app-up:
	make -j 2 backend-up frontend-up

backend-up:
	dotnet run --project src/Server/NetAppForVika.Server.csproj

frontend-up:
	cd frontend && npm run dev
