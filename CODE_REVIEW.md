# Code Review - OrderService

## 1. Análise detalhada do código original

### Problemas encontrados:

1. **Conexão estática e global**
   ```csharp
   public static SqlConnection conn = new SqlConnection("Server=.;Database=Orders;");
    ```
2. **Strings concatenadas em SQL**
   ```csharp
      var cmd = new SqlCommand("INSERT INTO Orders VALUES (" + customerId + ")", conn);
   ```
3. **Uso de MAX(Id) para pegar o último ID**
   ```csharp
      var cmd2 = new SqlCommand("SELECT MAX(Id) FROM Orders", conn);
   ```
4. **Uso de Thread.Sleep no loop**
5. **Inserção de itens sem transação**
6. **Exceção sem o throw**



