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


### Codifo Refatorado:
   ```csharp
      public class OrderService
      {
          private readonly string _connectionString;
      
          public OrderService(string connectionString)
          {
              _connectionString = connectionString;
          }
      
          public void CreateOrder(int customerId, List<int> productIds)
          {
              try
              {
                  using var conn = new SqlConnection(_connectionString);
                  conn.Open();
      
                  using var transaction = conn.BeginTransaction();
      
                  // Inserir pedido
                  var cmd = new SqlCommand(
                      "INSERT INTO Orders (CustomerId) VALUES (@customerId); SELECT SCOPE_IDENTITY();",
                      conn, transaction
                  );
                  cmd.Parameters.AddWithValue("@customerId", customerId);
                  int orderId = Convert.ToInt32(cmd.ExecuteScalar());
      
                  // Inserir itens
                  foreach (var productId in productIds)
                  {
                      var cmdItem = new SqlCommand(
                          "INSERT INTO OrderItems (OrderId, ProductId) VALUES (@orderId, @productId)",
                          conn, transaction
                      );
                      cmdItem.Parameters.AddWithValue("@orderId", orderId);
                      cmdItem.Parameters.AddWithValue("@productId", productId);
                      cmdItem.ExecuteNonQuery();
                  }
      
                  transaction.Commit();
              }
              catch (Exception ex)
              {
                  // Log de erro ou rethrow para tratamento externo
                  Console.Error.WriteLine($"Erro ao criar pedido: {ex.Message}");
                  throw ex;
              }
          }
      }
   ```



