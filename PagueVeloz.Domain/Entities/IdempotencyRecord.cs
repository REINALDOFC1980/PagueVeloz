using System;

namespace PagueVeloz.Domain.Entities
{
    /// <summary>
    /// Armazena respostas de requisições idempotentes para evitar duplicidade.
    /// </summary>
    public class IdempotencyRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Chave única enviada pelo cliente via header: Idempotency-Key
        /// </summary>
        public string Key { get; set; } = default!;

        /// <summary>
        /// Resposta completa devolvida ao cliente (JSON serializado)
        /// </summary>
        public string Response { get; set; } = default!;

        /// <summary>
        /// Data da criação do registro
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
