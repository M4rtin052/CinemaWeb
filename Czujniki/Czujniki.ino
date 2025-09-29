/*
Biblioteki zapewniane przez firmę Espressif Systems zapewniające wsparcie dla programowania mikrokontrolerów ESP32 w środowisku Arduino IDE
https://github.com/espressif/arduino-esp32
*/
#include <WiFi.h>
#include <HTTPClient.h>
/* 
Copyright (c) 2024 Miles Burton
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
https://github.com/milesburton/Arduino-Temperature-Control-Library/blob/master/LICENSE
*/
#include <DallasTemperature.h>
#include <OneWire.h>

#define LDR_PIN 34 // ESP32 pin GPIO34 (ADC1_CH6)
#define TEMP_DATA_PIN 4 // ESP32 pin GPIO4

#include "secret.h"
/*
Należy usunąć "#include secret.h", oraz odkomentować, aby dostosować do swojej sieci

const char* ssid = "NazwaSieci";
const char* password = "HasłoDoSieci";
const char* serverSetup = "http://192.168.x.x:5055/api/DataReceive"; // adres IPv4 komputera
*/

// Utworzenie instancji 'oneWire' do komunikacji
OneWire oneWire(TEMP_DATA_PIN);
// Utworzenie instancji 'sensors' i przekazanie, jako argumentu, wskaźnika do obiektu OneWire
DallasTemperature sensors(&oneWire);

void WiFisetup() {
  WiFi.mode(WIFI_STA); // Ustawienie płytki ESP32 w 1 z 3 trybów
  WiFi.begin(ssid, password); // Połączenie z siecią Wi-Fi
  Serial.print("Łączenie z siecią WiFi...");
  while (WiFi.status() != WL_CONNECTED) { // Pętla sprawdzająca czy połączono z siecią
    Serial.print('.');
    delay(1000);
  }
  Serial.println(WiFi.localIP()); // Wywołanie funkcji sprawdzającej unikalne IP płytki ESP32, nadane przez punkt dostępu (router)
}

void setup() {
  Serial.begin(115200); // Ustawienie wartości baudrate, określa szybkość transmiji (przeznaczone dla testów)

  sensors.begin(); // Wywołanie funkcji rozpoczynającej pracę czujnika temperatury
  WiFisetup(); // Wywołanie funkcji inicjalizującej łączenie z siecią Wi-Fi
}

String LDRsensor() {
  int LDRvalueRange = analogRead(LDR_PIN); // Odczyt wartości na wejściu analogowym czujnika LDR (zakres wartości pomiędzy 0 a 4095 - rozdzielczość 12-bitowa ADC 0 do 2^12 - 1 = 4095)
  String infoToServer;  // Utworzenie zmiennej typu string do przechowywania tekstu

  /*Kod przeznaczony do testowania odczytu danych*/
  Serial.print("Odczytana wartość z czujnika światła = "); 
  Serial.print(LDRvalueRange);

  /*Kod przeznaczony do sprawdzania i sortowania warunków oświetlenia*/
  if (LDRvalueRange < 3500) {
    Serial.println(" => Jasno");
    infoToServer = "Jasno";
  } 
  else if (LDRvalueRange < 3900) {
    Serial.println(" => Ciemno");
    infoToServer = "Ciemno";
  } 
  else {
    Serial.println(" => Bardzo ciemno");
    infoToServer = "Bardzo ciemno";
  }

  return infoToServer; // Zwracana zmienna typu string
}

float tempSensor() {
  sensors.requestTemperatures(); // Funkcja inicjalizująca pomiar temperatury
  float getTempInC = sensors.getTempCByIndex(0); // Pobranie danych z czujnika i zapisanie ich do zmiennej typu float

  /*Kod przeznaczony do testowania odczytu danych*/
  Serial.print(getTempInC);
  Serial.println("ºC");

  return getTempInC; // Zwracana zmienna typu float
}

void dataSending(String infoToServer, float getTempInC) { // Funkcja realizująca przesyłanie danych na serwer
  if (WiFi.status() == WL_CONNECTED) { // Warunek sprawdzający czy ESP32 dalej jest połączone z siecią Wi-Fi
    HTTPClient http; // Stworzenie obiektu 'http' klasy 'HTTPClient'
    
    http.begin(serverSetup); // Inicjowanie połączenia http z serwerem
    http.addHeader("Content-Type", "application/json"); // Dodanie nagłówka http do żądania - określa typ danych, tutaj JSON
    
    // Informacje, które mają zostać przesłane w postaci JSON
    String allDatasToSend = "{\"Temperature\": \"" + String(getTempInC, 2) + "\", \"Lightening\": \"" + String(infoToServer) + "\"}";
    int httpResponseCode = http.POST(allDatasToSend); // Wysyłanie informacji do serwera za pomocą metody POST, z jednoczesnym zapisaniem kodu odpowiedzi z serwera do zmiennej typu int

    /*Kod odpowiedzialny za testowanie uzyskiwanych odpowiedzi z serwera*/
    if (httpResponseCode > 0) { // Warunek sprawdzający kod odpowiedzi z serwera
      Serial.print("Odpowiedź HTTP: ");
      Serial.println(httpResponseCode);
    }
    else { // Warunek, który wykona się, gdy wystąpiły problemy przy wysyłaniu informacji do serwera
      Serial.print("Błąd wysyłania: ");
      Serial.println(httpResponseCode);
    }

    http.end(); // Zakończenie połączenia http w celu zwolnienia zajmowanej pamięci mikrokontrolera
  }
}

void loop() { // Główna pętla programu
  if (WiFi.status() == WL_CONNECTED) { // Warunek sprawdzający czy ESP32 dalej jest połączone z siecią Wi-Fi
    String infoToServer = LDRsensor(); // Wywołanie funkcji 'LDRsensor' i zapisanie zwracanych wartości do zmiennej typu string
    float getTempInC = tempSensor(); // Wywołanie funkcji 'tempSensor' i zapisanie zwracanych wartości do zmiennej typu float
    dataSending(infoToServer, getTempInC); // Wywołanie funkcji 'dataSending' z argumentami poprzednich zmiennych
  }
  else {  // Gdy połączenie z siecią Wi-Fi zostało utracone
    Serial.println("Wznawianie połączenia...");
    WiFi.disconnect(); // Rozłączenie z poprzednią siecią
    WiFi.reconnect(); // Próba łączenia z ostatnią znaną siecią
  }
  
  delay(30000); // Opoźnienie pół minuty
}