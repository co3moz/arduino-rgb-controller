void setup() {
    pinMode(11, OUTPUT);
    pinMode(10, OUTPUT);
    pinMode(9, OUTPUT);
    Serial.begin(9600);
}

int i[3] = {0, 0, 0};
void loop() {
    analogWrite(11, i[1]);
    analogWrite(10, i[0]);
    analogWrite(9, i[2]);
    if (Serial.available() >= 4) {
        byte a = Serial.read() - '0';
        byte b = Serial.read() - '0';
        byte c = Serial.read() - '0';
        byte x = Serial.read();
        if(x == 'r') x = 0;
        else if(x == 'g') x = 1;
        else if(x == 'b') x = 2;
        else return;
        i[x] = a * 100 + b * 10 + c;
        Serial.print("change color[");
        Serial.print(x == 0 ? 'r' : x == 1 ? 'g' : 'b');
        Serial.print("]=");
        Serial.print(i[x]);
        Serial.print('\n');
    }
}