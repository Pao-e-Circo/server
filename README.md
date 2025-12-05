A execução da aplicação pode ser feita através do Docker. Para isso, certifique-se que você possui o [Docker instalado](https://www.docker.com/products/docker-desktop/) em sua máquina.
   
1. Faça o download da aplicação na sua máquina local:
   
   ```
   git clone https://github.com/Pao-e-Circo/server.git  
   ```
  
2. Acesse o diretório da aplicação:

   ```
   $ cd server/
   ```
   
3. Execute a aplicação através do Docker Compose na raíz do projeto:

   ```
   $ docker compose -f compose.yml up -d
   ```

Caso os contâiners sejam pausados, execute o mesmo comando novamente para iniciá-los.
O Swagger UI é utilizado para a documentação de todos os endpoints do servidor. Para visualizá-lo, acesse `http://localhost:8080/swagger/index.html`.
