# Client Requirement Email Agent

An automated email agent that watches a Gmail inbox for new client requirement emails, analyzes them using a locally hosted LLM (Ollama), and emails a structured breakdown of the requirements (Functional, Non-Functional, Risks, Assumptions, and Questions to Client).

## Features
- Connects to your Gmail inbox via IMAP.
- Analyzes unstructured text to extract key business requirements.
- Uses `qwen2.5:3b-instruct` via [Ollama](https://ollama.ai/) running locally.
- Formats the response as an HTML email and sends it to a designated recipient.
- Prevents redundant processing by keeping track of the highest email UID and strictly ignoring older emails upon startup.

## Prerequisites
- Python 3.x
- A Gmail account with IMAP enabled.
- Google Account **App Password** (Requires 2-Step Verification to be enabled).
- [Ollama](https://ollama.ai/) installed locally.

## Setup Instructions

1. **Pull the Local LLM:**
   Ensure Ollama is running and download the necessary model:
   ```bash
   ollama pull qwen2.5:3b-instruct
   ```

2. **Set up the Virtual Environment & Dependencies:**
   ```bash
   python3 -m venv .venv
   source .venv/bin/activate
   pip install -r requirements.txt
   ```

3. **Configure Environment Variables:**
   Create a `.env` file in the root directory and provide your details:
   ```env
   GMAIL_ADDRESS=your_email@gmail.com
   GMAIL_APP_PASSWORD=your_16_character_app_password
   RECIPIENT_EMAIL=recipient_email@gmail.com
   ```
   *(Note: Do NOT use your regular Gmail password. Generate an App Password from your Google Account settings).*

## Usage

**Continuously Watch Inbox:**
Will actively monitor your inbox. The script safely ignores any older emails received *before* it started running.
```bash
python requirement_agent.py --watch
```

**Run Once For a Local Text File:**
```bash
python requirement_agent.py --file requirement.txt
```
