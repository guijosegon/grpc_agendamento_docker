# Sistema gRPC de Agendamento com Docker

Este repositório contém três micro-serviços gRPC em .NET 8, comunicando-se via HTTP/2 dentro de containers Docker:

1. **UserService**

   * Cadastro de usuários
2. **ScheduleService**

   * Criação / cancelamento / listagem de reservas (persistidas em `reservations.json` via volume Docker)
   * Upload / download de anexos (arquivo binário em `/data`)
   * Disparo de notificações ao criar uma reserva
3. **NotificationService**

   * Recebe e registra (em log) notificações vindas do ScheduleService

---

## 📚 Tecnologias

* .NET 8 + C#
* gRPC (Grpc.AspNetCore, Grpc.Tools, Google.Protobuf)
* HTTP/2 plaintext dentro do Docker
* Persistência simples em JSON via `System.Text.Json` no volumes do Docker
* Docker & Docker Compose

---

## 🚀 Como executar

1. Clone este repositório.
2. No terminal, na raiz do projeto:

   ```bash
   docker-compose up --build
   ```
3. Os serviços estarão disponíveis em:

   * UserService: `http://localhost:5001`
   * ScheduleService: `http://localhost:5002`
   * NotificationService:`http://localhost:5003`

> Cada serviço expõe um método `"/"` HTTP GET que retorna uma mensagem de saúde.

---

## 🔧 Endpoints gRPC & JSON

Você pode usar o Postman (gRPC) ou qualquer client gRPC que aceite JSON.

### UserService (porta 5001)

* **CreateUser**

  ```json
  {
    "name": "Alice",
    "email": "alice@example.com",
    "pwd": "senha123"
  }
  ```

  Retorna `{ userId, name, email }`.

* **Authenticate**

  ```json
  {
    "email": "alice@example.com",
    "pwd": "senha123"
  }
  ```

  Retorna `{ token, expires }` (JWT).

### ScheduleService (porta 5002)

* **CreateReservation**

  ```json
  {
    "start_time": "2025-06-24T10:00:00Z",
    "end_time": "2025-06-24T11:00:00Z",
    "user_id": 1,
    "description": "Exemplo",
  }
  ```

  → dispara `SendNotification` e retorna reserva.

* **CancelReservation**

  ```json
  { "reservation_id": 1 }
  ```

* **ListReservations**

  ```json
  { "user_id": 1 }
  ```

  Retorna lista de reservas.

* **UploadAttachment** (stream de chunks)
  Envie vários JSONs:

  ```jsonc
  { "filename": "doc.pdf", "data": "<base64>" }
  { "data": "<base64>" }
  ```

  Retorna `{ success, message }`.

* **DownloadAttachment**

  ```json
  { "filename": "doc.pdf" }
  ```

  Retorna stream de chunks `{ filename?, data }`.

### NotificationService (porta 5003)

* **SendNotification**

  ```json
  {
    "user_id": 1,
    "message": "Reserva #1 confirmada"
  }
  ```

  Retorna `{ sent: true }`.

> **Obs.**: este serviço não exige autenticação; as chamadas vêm internamente do ScheduleService.

---

## 🔄 Persistência em JSON

Cada serviço que precisa de durabilidade monta um volume Docker:

* **UserService** → `/data/users.json` via volume `user-data`
* **ScheduleService** → `/data/reservations.json` e `/data/<attachments>` via `schedule-data`
* **NotificationService** não persiste em arquivo

Você pode inspecionar os volumes no Docker Desktop (Volumes ➔ “Explore”) ou:

```bash
docker run --rm -v grpc_agendamento_user-data:/data alpine sh -c "ls /data && cat /data/users.json"
```

---

## 🎓 Uso em produção

* Substituir repositórios em JSON por bancos (SQLite, PostgreSQL, etc.)
* Habilitar HTTPS / certificados
* Configurar *circuit breakers*, timeouts, retrys no client gRPC
* Adicionar *metrics*, *tracing* e *logging* estruturado em cada service

---

© 2025 — Projeto de Agendamento gRPC para Backend-to-Backend
