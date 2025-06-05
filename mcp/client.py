from google import genai
from google.genai import types
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

client=genai.Client(api_key="")

server_params = StdioServerParameters(
    command="uv",
    args=["--connection_type", "stdio"],
)
