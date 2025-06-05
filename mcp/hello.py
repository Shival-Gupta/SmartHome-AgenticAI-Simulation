from mcp.server.fastmcp import FastMCP
import httpx

mcp=FastMCP("home")


# BASE_URL = "https://4cbe-2409-40f4-aa-d640-f810-fdcf-176e-e592.ngrok-free.app"  
BASE_URL = "http://localhost:5000/"  


def control_fan_tool(state: bool) -> str:
    """Control the fan. 1 = ON, 0 = OFF"""
    try:
        response = httpx.post(f"{BASE_URL}/control_fan", json={"state": state})
        response.raise_for_status()  # Raise an exception for HTTP errors
        return response.json().get("message", "Error: Unable to control fan")
    except httpx.RequestError as exc:
        return f"Error: Unable to connect to the server ({exc})"
    except httpx.HTTPStatusError as exc:
        return f"Error: {exc.response.status_code} - {exc.response.text}"


def control_light_tool(state: bool) -> str:
    """Control the light. 1 = ON, 0 = OFF"""
    try:
        response = httpx.post(f"{BASE_URL}/control_light", json={"state": state})
        response.raise_for_status()
        return response.json().get("message", "Error: Unable to control light")
    except httpx.RequestError as exc:
        return f"Error: Unable to connect to the server ({exc})"
    except httpx.HTTPStatusError as exc:
        return f"Error: {exc.response.status_code} - {exc.response.text}"


def control_uv_sensor_tool(state: bool) -> str:
    """Enable or disable UV sensor."""
    try:
        response = httpx.post(f"{BASE_URL}/control_uv_sensor", json={"state": state})
        response.raise_for_status()
        return response.json().get("message", "Error: Unable to control UV sensor")
    except httpx.RequestError as exc:
        return f"Error: Unable to connect to the server ({exc})"
    except httpx.HTTPStatusError as exc:
        return f"Error: {exc.response.status_code} - {exc.response.text}"


def set_display_digit_tool(digit: int) -> str:
    """Set the digit on the single-digit display."""
    try:
        response = httpx.post(f"{BASE_URL}/set_display_digit", json={"digit": digit})
        response.raise_for_status()
        return response.json().get("message", "Error: Unable to set display digit")
    except httpx.RequestError as exc:
        return f"Error: Unable to connect to the server ({exc})"
    except httpx.HTTPStatusError as exc:
        return f"Error: {exc.response.status_code} - {exc.response.text}"


@mcp.tool()
def control_fan(state: bool) -> str:
    """Control the fan. True = ON, False = OFF"""
    response = control_fan_tool(state)
    return response

@mcp.tool()
def control_light(state: bool) -> str:
    """Control the light. True = ON, False = OFF"""
    response = control_light_tool(state)
    return response

@mcp.tool()
def control_uv_sensor(state: bool) -> str:
    """Enable or disable UV sensor."""
    response = control_uv_sensor_tool(state)
    return response

@mcp.tool()
def set_display_digit(digit: int) -> str:
    """Set the digit on the single-digit display."""
    response = set_display_digit_tool(digit)
    return response




if __name__ == "__main__":
    mcp.run(transport="stdio")
