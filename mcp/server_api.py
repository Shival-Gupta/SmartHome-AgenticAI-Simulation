from flask import Flask, request, jsonify
from serial_sender import control_fan, control_light, control_uv_sensor, set_display_digit

app = Flask(__name__)

@app.route('/control_fan', methods=['POST'])
def handle_control_fan():
    data = request.json
    state = data.get('state')
    if state is None:
        return jsonify({"error": "State is required"}), 400
    result = control_fan(state)
    return jsonify({"message": result})

@app.route('/control_light', methods=['POST'])
def handle_control_light():
    data = request.json
    state = data.get('state')
    if state is None:
        return jsonify({"error": "State is required"}), 400
    result = control_light(state)
    return jsonify({"message": result})

@app.route('/control_uv_sensor', methods=['POST'])
def handle_control_uv_sensor():
    data = request.json
    state = data.get('state')
    if state is None:
        return jsonify({"error": "State is required"}), 400
    result = control_uv_sensor(state)
    return jsonify({"message": result})

@app.route('/set_display_digit', methods=['POST'])
def handle_set_display_digit():
    data = request.json
    digit = data.get('digit')
    if digit is None:
        return jsonify({"error": "Digit is required"}), 400
    result = set_display_digit(digit)
    return jsonify({"message": result})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)