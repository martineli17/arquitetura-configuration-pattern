# Configuration Pattern
<p>Quando criamos aplicações, na maior parte das vezes é necessário que a aplicação utilize de alguma variável de ambiente para, por exemplo, acessar determinado serviço ou realizar algum processamento.</p>
<p>Dentro deste cenário, há a possibilidade de alterações dessas variáveis de ambiente, umas com mais frequência e outras com menos.</p>
<p>Em alguns casos, é comum que a definição ocorra no momento do deploy da aplicação dentro de uma esteira CI/CD, o que deixa os valores fixos para cada deploy. Com isso, caso ocorra a necessidade de modificar alguma das varáiveis utilizadas pela aplicação, seria necessário executar novamente a esteira de CI/CD e um novo deploy seria feito.</p>
<p>Não há grandes problemas nesta implementação, porém um impacto negativo é que a cada mudança, o app pode ficar momentos indisponíveis até a nova versão for publicada e estar pronta para execução.</p>
<p>Para evitar tal indisponibilidade, o Pattern Configuration pode ajudar, visto que o intuito dele é tornar a definição/alteração de variáveis de ambiente mais dinâmica e disponíveis o quanto antes para a sua aplicação utilizar, visando evitar um novo deploy para isso.</p>

## Como utilizar
<p>Para que seja possível modificar alguma das variáveis de ambiente e as mesmas já estarem disponíveis com o novo valor para a aplicação utilizar, existe alguns métodos para isso: </p>
<ul>
  <li>Realizar a atualização das variáveis através de um endpoint REST</li>
  <li>Realizar a atualização das variáveis através de mensageria (utilizada neste exemplo) </li>
</ul>
<p>No exemplo criado, foi utilizado a mensageria para realizar a atualização. No momento do build da aplicação, é registrado um subscriber para uma fila do RabbitMq que fica responsável por atualizar as variáveis a cada nova mensagem publicada.</p>
<p>Supondo que exista N microsserviços, é possível criar um tópico específico e, cada um dos microsserviços ter uma subscription para este tópico. Com isso, quando uma nova mensagem for enviada para o tópico, todos os microsserviços que estão registrados serão atualizados em paralelo (tirando a necessidade de atualizar cada um separadamente).</p>

## Configuração inicial
<p>Quando utilizamos microsserviços, normalmente é implementado algum serviço kubernetes para o deploy dos mesmos. Sendo assim, um pod pode ser excluído e criado várias vezes. </p>
<p>Tendo este cenário em mente, caso um pod seja criado novamente, o mesmo precisa ter acesso ao valor mais atualizadas das variáveis de ambiente. Isso faz surgir a necessidade de centralizar as variáveis em algum Secret Management e, no momento do build da sua aplicação, ela solicitar acesso a tais variáveis, garantindo que quando um pod é criado o app não utilizará valores desatualizados.</p>
<p>Neste exemplo, existe um método que solicita as variáveis de ambiente para o Secret Management no momento do build da aplicação, definindo assim os valores iniciais.</p>

## [Secrement Management](https://docs.doppler.com/docs)
<p>Foi utilizado para esta implementação, o Secrement Management Doppler</p>
