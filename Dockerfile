# 1️⃣ Usa Node.js oficial, versão leve (Alpine)
FROM node:18-alpine

# 2️⃣ Define o diretório de trabalho dentro do container
WORKDIR /app

# 3️⃣ Copia apenas os arquivos de dependência para aproveitar o cache do Docker
COPY package*.json ./

# 4️⃣ Instala as dependências do projeto
RUN npm install

# 5️⃣ Copia o restante do código da aplicação
COPY . .

# 6️⃣ Adiciona node_modules/.bin ao PATH
ENV PATH=/app/node_modules/.bin:$PATH

# 7️⃣ Expõe a porta em que a aplicação vai rodar
EXPOSE 3000

# 8️⃣ Comando para iniciar a aplicação
CMD ["npm", "start"]
