# Local environment setup

The application reads shared configuration from the repository-root `.env` file.

1. Copy the example file:
   ```bash
   cp .env.example .env
   ```
2. Add your OpenAI API key to `OPENAI_API_KEY`.
3. If you are testing MTN MoMo sandbox operations, add the sandbox credentials to the `MOMO_*` variables.
4. Start the backend agents and frontend using the commands in the main [README](README.md).

Never commit `.env` or share API keys in screenshots, logs, source files or public repositories. If a credential has already been committed or shared, revoke or rotate it with the provider.
