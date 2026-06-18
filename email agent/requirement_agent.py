#!/usr/bin/env python3
"""
Client Requirement Analyzer & Mailer
=====================================

Workflow
--------
1. Reads a client requirement (from a local .txt file, or directly from a
   new email in your inbox).
2. Sends the requirement text to a locally running Ollama model
   (e.g. qwen3.5-instruct) and asks it to produce:
       - Functional Requirements
       - Non-Functional Requirements
       - Risks
       - Assumptions
       - Questions to Client
3. Formats the result as an HTML email.
4. Sends that email to another Gmail address.

SETUP
-----
1. Install & run Ollama, then pull the model you want to use, e.g.:
       ollama pull qwen3.5:9b-instruct
       ollama pull qwen3.5-instruct
   Run `ollama list` afterwards and copy the EXACT name/tag shown into
   OLLAMA_MODEL below.

2. Install the only extra dependency:
       pip install requests

3. Gmail setup (needed for sending, and for --watch mode reading):
   - Go to https://myaccount.google.com/apppasswords and create an
     "App Password" for this script (your normal Gmail password will
     NOT work with smtplib/imaplib).
   - If using --watch mode, make sure IMAP is enabled in
     Gmail Settings -> Forwarding and POP/IMAP.

4. Fill in the CONFIG section below with your real values.

USAGE
-----
One-shot, from a local text file (e.g. the requirement saved from
the client's email):
    python requirement_agent.py --file requirement.txt

Continuously watch your inbox and auto-process new client emails:
    python requirement_agent.py --watch
"""

import argparse
import email
import imaplib
import json
import os
import re
import smtplib
import sys
import time
from email.header import decode_header
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText

import requests
from dotenv import load_dotenv

load_dotenv()

# ------------------------------------------------------------------
# CONFIG -- edit these values for your setup
# ------------------------------------------------------------------

# --- Ollama (local model) ---
OLLAMA_URL = "http://localhost:11434/api/generate"
OLLAMA_MODEL = "qwen2.5:3b-instruct"  # must match the name from `ollama list`

# --- Gmail account used to receive client mail AND send the analysis ---
GMAIL_ADDRESS = os.environ.get("GMAIL_ADDRESS", "")
GMAIL_APP_PASSWORD = os.environ.get("GMAIL_APP_PASSWORD", "")

# --- Where the analysis email should be sent ---
RECIPIENT_EMAIL = os.environ.get("RECIPIENT_EMAIL", "")

# --- Only process emails from this sender ("" = process from anyone) ---
CLIENT_SENDER_FILTER = ""

# --- How often to check the inbox in --watch mode (seconds) ---
POLL_INTERVAL_SECONDS = 60

IMAP_HOST = "imap.gmail.com"
SMTP_HOST = "smtp.gmail.com"
SMTP_PORT = 465


# ------------------------------------------------------------------
# OLLAMA CALL
# ------------------------------------------------------------------
ANALYSIS_PROMPT = """You are an experienced business analyst reviewing a client's project requirement email.

Read the requirement text below and produce a structured analysis.

Requirement text:
\"\"\"
{requirement}
\"\"\"

Respond with ONLY a JSON object (no markdown, no code fences, no extra commentary) using exactly this schema:
{{
  "functional_requirements": ["...", "..."],
  "non_functional_requirements": ["...", "..."],
  "risks": ["...", "..."],
  "assumptions": ["...", "..."],
  "questions_to_client": ["...", "..."]
}}

Each list item should be a single clear, concise sentence. Provide at least 3 items per list where reasonably possible.
"""


def call_ollama(requirement_text: str) -> dict:
    """Send the requirement text to the local Ollama model and parse its JSON reply."""
    prompt = ANALYSIS_PROMPT.format(requirement=requirement_text)

    payload = {
        "model": OLLAMA_MODEL,
        "prompt": prompt,
        "stream": False,
        "format": "json",  # ask Ollama to constrain output to valid JSON
        "options": {"temperature": 0.2},
    }

    resp = requests.post(OLLAMA_URL, json=payload, timeout=600)
    resp.raise_for_status()
    raw = resp.json().get("response", "")

    return _parse_json_response(raw)


def _parse_json_response(raw: str) -> dict:
    """Best-effort extraction of a JSON object from the model's raw text."""
    raw = raw.strip()
    # strip ```json ... ``` fences in case the model added them anyway
    raw = re.sub(r"^```(?:json)?\s*|\s*```$", "", raw, flags=re.MULTILINE).strip()

    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        match = re.search(r"\{.*\}", raw, flags=re.DOTALL)
        if match:
            return json.loads(match.group(0))
        raise ValueError(f"Could not parse JSON from model output:\n{raw}")


# ------------------------------------------------------------------
# EMAIL FORMATTING
# ------------------------------------------------------------------
SECTION_TITLES = {
    "functional_requirements": "Functional Requirements",
    "non_functional_requirements": "Non-Functional Requirements",
    "risks": "Risks",
    "assumptions": "Assumptions",
    "questions_to_client": "Questions to Client",
}


def build_email_body(analysis: dict, source_excerpt: str = "") -> str:
    """Build an HTML email body from the analysis dict."""
    parts = ["<h2>Requirement Analysis</h2>"]

    if source_excerpt:
        parts.append(
            f"<p><strong>Source excerpt:</strong><br>{source_excerpt}</p><hr>"
        )

    for key, title in SECTION_TITLES.items():
        items = analysis.get(key, [])
        parts.append(f"<h3>{title}</h3>")
        if items:
            parts.append("<ul>")
            for item in items:
                parts.append(f"<li>{item}</li>")
            parts.append("</ul>")
        else:
            parts.append("<p><em>None identified.</em></p>")

    return "\n".join(parts)


# ------------------------------------------------------------------
# SENDING THE RESULT EMAIL
# ------------------------------------------------------------------
def send_email(subject: str, html_body: str, to_address: str):
    msg = MIMEMultipart("alternative")
    msg["From"] = GMAIL_ADDRESS
    msg["To"] = to_address
    msg["Subject"] = subject
    msg.attach(MIMEText(html_body, "html"))

    with smtplib.SMTP_SSL(SMTP_HOST, SMTP_PORT) as server:
        server.login(GMAIL_ADDRESS, GMAIL_APP_PASSWORD)
        server.sendmail(GMAIL_ADDRESS, to_address, msg.as_string())

    print(f"[OK] Analysis email sent to {to_address}")


# ------------------------------------------------------------------
# READING NEW CLIENT EMAILS (IMAP) -- used in --watch mode
# ------------------------------------------------------------------
def _decode(value):
    decoded, encoding = decode_header(value)[0]
    if isinstance(decoded, bytes):
        return decoded.decode(encoding or "utf-8", errors="ignore")
    return decoded


_START_UID = None

def get_start_uid() -> int:
    """Find the highest UID currently in the INBOX to ignore older emails."""
    with imaplib.IMAP4_SSL(IMAP_HOST) as imap:
        imap.login(GMAIL_ADDRESS, GMAIL_APP_PASSWORD)
        imap.select("INBOX")
        status, data = imap.uid('SEARCH', None, "ALL")
        if status == "OK" and data[0]:
            uids = data[0].split()
            if uids:
                return int(uids[-1])
    return 0

def fetch_new_client_emails():
    """Return a list of (subject, body_text) for unread emails matching the filter."""
    global _START_UID
    if _START_UID is None:
        _START_UID = get_start_uid()
        print(f"[INFO] Ignoring all existing emails (up to UID {_START_UID})")

    results = []
    with imaplib.IMAP4_SSL(IMAP_HOST) as imap:
        imap.login(GMAIL_ADDRESS, GMAIL_APP_PASSWORD)
        imap.select("INBOX")

        criteria = f"(UNSEEN UID {_START_UID + 1}:*)"
        if CLIENT_SENDER_FILTER:
            criteria = f'(UNSEEN FROM "{CLIENT_SENDER_FILTER}" UID {_START_UID + 1}:*)'

        status, data = imap.uid('SEARCH', None, criteria)
        if status != "OK" or not data[0]:
            return results

        for uid_bytes in data[0].split():
            uid_int = int(uid_bytes)
            # IMAP range queries might return the largest UID even if it's smaller than requested
            if uid_int <= _START_UID:
                continue

            status, msg_data = imap.uid('FETCH', uid_bytes, "(RFC822)")
            if status != "OK":
                continue
            
            if isinstance(msg_data[0], tuple):
                msg = email.message_from_bytes(msg_data[0][1])
                subject = _decode(msg.get("Subject", "(no subject)"))
                body = _extract_body(msg)
                results.append((subject, body))
                imap.uid('STORE', uid_bytes, "+FLAGS", "\\Seen")  # don't reprocess it next time

    return results


def _extract_body(msg) -> str:
    body_text = ""
    attachment_text = ""
    if msg.is_multipart():
        for part in msg.walk():
            content_type = part.get_content_type()
            filename = part.get_filename()
            
            if content_type == "text/plain" and not filename:
                charset = part.get_content_charset() or "utf-8"
                payload = part.get_payload(decode=True)
                if payload:
                    body_text += payload.decode(charset, errors="ignore") + "\n"
            elif filename and filename.lower().endswith(".txt"):
                charset = part.get_content_charset() or "utf-8"
                payload = part.get_payload(decode=True)
                if payload:
                    attachment_text += f"\n--- Attachment: {filename} ---\n"
                    attachment_text += payload.decode(charset, errors="ignore") + "\n"
                    
        return (body_text + "\n" + attachment_text).strip()
    else:
        charset = msg.get_content_charset() or "utf-8"
        return msg.get_payload(decode=True).decode(charset, errors="ignore")


# ------------------------------------------------------------------
# MAIN PIPELINE
# ------------------------------------------------------------------
def process_requirement(text: str, subject_hint: str = "Client Requirement"):
    print("Sending requirement text to local Ollama model for analysis...")
    analysis = call_ollama(text)

    excerpt = (text[:300] + "...") if len(text) > 300 else text
    body = build_email_body(analysis, source_excerpt=excerpt)

    subject = f"Requirement Analysis: {subject_hint}"
    send_email(subject, body, RECIPIENT_EMAIL)


def run_once_from_file(path: str):
    with open(path, "r", encoding="utf-8") as f:
        text = f.read()
    process_requirement(text, subject_hint=path)


def watch_inbox():
    print(
        f"Watching {GMAIL_ADDRESS} for new client emails "
        f"every {POLL_INTERVAL_SECONDS}s ... (Ctrl+C to stop)"
    )
    while True:
        try:
            for subject, body in fetch_new_client_emails():
                print(f"New requirement email: {subject}")
                if body.strip():
                    process_requirement(body, subject_hint=subject)
                else:
                    print("  (empty body, skipped)")
        except Exception as exc:
            print(f"[ERROR] {exc}", file=sys.stderr)

        time.sleep(POLL_INTERVAL_SECONDS)


# ------------------------------------------------------------------
if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Client requirement analysis agent")
    parser.add_argument("--file", help="Process a local requirement .txt file once")
    parser.add_argument(
        "--watch", action="store_true", help="Continuously watch inbox for client emails"
    )
    args = parser.parse_args()

    if args.file:
        run_once_from_file(args.file)
    elif args.watch:
        watch_inbox()
    else:
        parser.print_help()
