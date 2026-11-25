import os
import uuid
from pathlib import Path

import requests
from dotenv import load_dotenv

load_dotenv(Path(__file__).resolve().parents[1] / ".env")

# === CONFIG ===
SUBSCRIPTION_KEY = os.getenv("MOMO_SUBSCRIPTION_KEY")
PRODUCT = "collection"                       # "collection", "disbursement" or "remittance"
BASE_URL = "https://sandbox.momodeveloper.mtn.com"

def create_api_user():
    """Step 1: Create API User with unique UUID"""
    user_id = str(uuid.uuid4())
    url = f"{BASE_URL}/v1_0/apiuser"
    headers = {
        "Ocp-Apim-Subscription-Key": SUBSCRIPTION_KEY,
        "Content-Type": "application/json",
        "X-Reference-Id": user_id,
    }
    data = {"providerCallbackHost": "localhost"}

    resp = requests.post(url, headers=headers, json=data)
    if resp.status_code not in (201, 202):
        raise Exception(f"Failed to create API user: {resp.status_code}, {resp.text}")

    return user_id

def generate_api_key(user_id: str):
    """Step 2: Generate API Key for created user"""
    url = f"{BASE_URL}/v1_0/apiuser/{user_id}/apikey"
    headers = {
        "Ocp-Apim-Subscription-Key": SUBSCRIPTION_KEY,
        "Content-Length": "0",
    }
    resp = requests.post(url, headers=headers)
    if resp.status_code != 201:
        raise Exception(f"Failed to generate API key: {resp.status_code}, {resp.text}")

    return resp.json()["apiKey"]

if __name__ == "__main__":
    if not SUBSCRIPTION_KEY:
        raise SystemExit("MOMO_SUBSCRIPTION_KEY is missing. Add it to the repository-root .env file.")

    print("🚀 Creating Sandbox API User...")
    user_id = create_api_user()
    print(f"✅ API User created: {user_id}")

    print("🔑 Generating API Key...")
    api_key = generate_api_key(user_id)
    print(f"✅ API Key generated: {api_key}")

    print("\n📄 Add these to your .env file:")
    print(f"MOMO_USER_ID={user_id}")
    print(f"MOMO_API_KEY={api_key}")
    print(f"MOMO_SUBSCRIPTION_KEY={SUBSCRIPTION_KEY}")
    print(f"MOMO_ENVIRONMENT=sandbox")
    print(f"MOMO_PRODUCT={PRODUCT}")
