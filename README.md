OrderManager

## Descrição do Projeto

O **OrderManager** é uma API para gerenciamento de pedidos, feito em .NET 9 com arquitetura Clean Architecture. 
O projeto é dividido em camadas:

- **WEB API**: Interface de entrada (camada de apresentação) que expõe os endpoints REST.
- **Application**: Contém os casos de uso e serviços de aplicação.
- **Domain**: Contém entidades de negócio, agregados e regras de domínio.
- **Infrastructure**: Implementação de persistência (Entity Framework Core / PostgreSQL), serviços externos e repositórios.
- **Messaging**: Configuração de mensageria (MassTransit + RabbitMQ) e integração assíncrona.
- **Tests**: Testes unitários e de integração organizados em `tests/`.

A arquitetura segue princípios de separação de responsabilidades, inversão de dependência e utilização de DDD (Domain-Driven Design).

---

## Setup e Execução

### Pré-requisitos

- Docker e Docker Compose instalados
- .NET 9 SDK (para rodar localmente sem Docker)

### Rodando o projeto com Docker Compose

Na raiz do projeto, execute:

```bash
docker-compose up --build -d
```
Isso cria e sobe os containers:

PostgreSQL
Redis
RabbitMQ
OrderManager API

A API estará disponível em:
HTTP: http://localhost:5000/swagger/index.html
HTTPS: https://localhost:5001/swagger/index.html


// Para rodar os testes 
```bash
docker-compose run --rm tests
```


Melhorias Futuras e Débitos Técnicos

-Versionamento de API
-Logging distribuído (ex: ELK, Seq)
-Monitoramento de metricas (Grafana)
-Revisão da seed de dados para ambientes de produção
-Testes de carga e performance
-Refatoração de serviços para maior desacoplamento

