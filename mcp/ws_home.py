from mcp.server.fastmcp import FastMCP
import httpx
import json
import websockets
import asyncio

app = FastMCP("home")

BASE_URL = "ws://localhost:8080/iot"

@app.tool()
def read_document() -> str:
    """Fetches API document to understand the payloadb structure for controlling the home device using the API calls"""
    with open("API.md", "r") as file:
        document_content = file.read()
    print(document_content)
    return f"{str(document_content)}"

@app.tool()
async def send_payload(payload) -> dict:
    """Controls the home appliances by sending the payload to the WebSocket API according to the documentation, the input for this function should be in dictionary"""
    # Convert payload to a JSON string
    payload_str = json.dumps(payload)
    
    try:
        # Establish WebSocket connection and send the payload
        async with websockets.connect(BASE_URL) as websocket:
            await websocket.send(payload_str)
            # Optionally, receive a response if the WebSocket server sends one
            response = await websocket.recv()
            return json.loads(response) if response else {"status": "sent"}
    except Exception as e:
        return {"error": f"Failed to send payload: {str(e)}"}


if __name__ == "__main__":
    app.run(transport="stdio")