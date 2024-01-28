# Renacci
Serving the modbus of your Renac inverter as REST API.

This is totally work in progress! 

## Run
To run this in docker use: 

```docker run -it --rm -p 8080:8080 -e RenacModbusOptions__Host='<MODBUS-ADAPTER-IP>' -e RenacModbusOptions__Port=<MODBUS-ADAPTER-PORT> -e RenacModbusOptions__DeviceId=<INVERTER-DEVICE-ID> ghcr.io/yety/renacci:main```

Then call: http://localhost:8080/api/Status/GetStatus
