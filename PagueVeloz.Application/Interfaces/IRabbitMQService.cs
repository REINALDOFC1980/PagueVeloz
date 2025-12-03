namespace WebApiBiblioteca.Service.RabbitMQ
{
    public interface IRabbitMQService
    {
        void PublicarMensagem(object mensagem, string fila);
    }
}
