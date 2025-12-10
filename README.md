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

- PostgreSQL
- Redis
- RabbitMQ
- OrderManager API

A API estará disponível em:
- HTTP: http://localhost:5000/swagger/index.html
- HTTPS: https://localhost:5001/swagger/index.html

### Para rodar os testes com Docker
```bash
docker-compose run --rm tests
```

### Rodando Localmente (Sem Docker)
Use o comando para atualizar o DB a partir da migration já criada:

```bash
dotnet ef database update --project src/OrderManager.Infrastructure --startup-project src/OrderManager.Web.Api
```

Depois rode a API:

```bash
dotnet run --project src/OrderManager.Web.Api
```

### Melhorias Futuras e Débitos Técnicos

- Versionamento de API
- Logging distribuído (ex: ELK, Seq)
- Monitoramento de metricas (Grafana)
- Revisão da seed de dados para ambientes de produção
- Testes de carga e performance
- Refatoração de serviços para maior desacoplamento


## Questões Teóricas

1. Sistema de cache distribuído para consultas de pedidos

Um cache distribuído guarda os resultados das consultas para que não precisemos ir ao banco de dados toda vez. Por exemplo, se alguém pedir os detalhes de um pedido, podemos salvar no Redis.
Para manter os dados corretos, usamos invalidação:
- Expiração automática (TTL): o cache "expira" depois de um tempo e os dados são recarregados do banco.
- Invalidação ativa: quando um pedido é atualizado ou cancelado, removemos ou atualizamos o cache desse pedido imediatamente.

2. Garantindo consistência eventual entre pedidos e estoque

Quando temos serviços diferentes (como pedidos e estoque), nem sempre os dados ficam sincronizados. A consistência eventual significa que com o tempo eles se alinham.
Para isso, podemos usar:
- Eventos assíncronos: quando um pedido é criado ou atualizado, o serviço de pedidos envia um evento. O serviço de estoque lê esse evento e atualiza o estoque.
- Padrão Saga: cria uma "história" de ações distribuídas com compensações caso algo falhe. Exemplo, se atualizar o estoque falhar, dá para desfazer o pedido.

3. Mecanismo de retry resiliente para integrações externas

Para serviços externos que falham, podemos tentar novamente de forma inteligente
- Retry simples: tentar 2-3 vezes se der erro.
- Backoff: esperar um tempo maior a cada tentativa (5s, 10s, 20s).
- Circuit breaker: se muitas falhas acontecerem, para de tentar por um tempo para não sobrecarregar o serviço externo.

4. Refatorando um método monolítico de 500 linhas

- Começar dividindo em partes menores
- Identificar blocos de lógica e criar funções separadas.
- Aplicar o S do Solid, cada função faz só uma coisa.
- Criar classes ou serviços para responsabilidades diferentes (ex: validação, cálculo, persistência).
- Testar cada parte separadamente para garantir que nada esta com bug.

5. Lidando com deadlocks em alta concorrência

Deadlock acontece quando duas transações ficam presas, esperando uma pela outra.
Para resolver:
- Ordenar acesso aos recursos: sempre acessar tabelas ou registros na mesma ordem.
- Timeouts e retries: se uma transação travar, tentar novamente.
- Monitoramento: usar logs do banco ou ferramentas de profiling para identificar onde ocorrem os deadlocks.

