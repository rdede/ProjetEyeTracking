# Guide d'utilisation

**Attention : Il est nécessaire de lancer le projet WebApp (Serveur Web) avant le projet EyeTracking (Client Tobii)**

## Mise en place

- Télécharger et installer BUS USB Service 1.3.2 (USB-HID firmware)
- Télécharger et installer Tobii EyeX dans sa version 2.1.1
- Brancher et calibrer le Eyetracker Tobii Rex for Developers
- Le logiciel Tobii EyeX doit tourner et la capture doit être activée

## Lancement du Web Server

- Ouvrir le projet WebApp.sln avec Visual Studio (2022 de préférence)
- Lancer le projet, une page localhost:8080 devrait s'ouvrir dans le navigateur
- La console doit afficher "En attente du lancement du client Tobii..." et la page web doit charger dans le vide 

## Lancement du Client Tobii

- Ouvrir le projet ProjetEyeTracking.sln
- Lancer le projet, la console devrait alors afficher "Socket Connected [...]"
- La page web devrait alors charger et afficher une interface permettant de commencer l'expérience

## Configuration

- Les images se trouvent dans le dossier "wwwroot/img" du projet WebApp, il est possible de les modifier mais il faudra conserver la nomenclature actuelle (img[1-9].jpg)
- Les fichiers JSON générés se trouvent dans le dossier "output" du client ProjetEyeTracking