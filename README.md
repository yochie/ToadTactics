# Installation

* Installer unity hub : https://unity.com/download
* Depuis unity hub, installer la version de unity 2021.3.26f1
* Créer un compte https://github.com/ et me demander l'accès à notre repo github
* Installer https://gitforwindows.org/ 
* Installer https://github.com/git-lfs/git-lfs/releases
* Naviguer à l'endroit dans votre ordi où vous voulez installer le projet et faire click-droit = > git bash here
* Cloner le repo avec la commande : `git clone https://github.com/yochie/thom.git`
* Rentrer dans le dossier depuis la ligne de commande : `cd thom`
* Finaliser l'installation git LFS avec la commande : `git lfs install`
* Ouvrir Unity hub et selectionner l'option "Open" => "Open project from disk"
* Ouvrir le projet et ignorer les messages d'erreures et le mode safe pour commencer, il manque une librairie, c'est normal
* Pour installer la librairie Mirror (networking)  : Menu "Window" => Asset store (Open in browser), rechercher "Mirror" => "Add to my assets" => "Open in Unity"
* De retour dans unity, selectionner le package Mirror dans la fenêtre qui s'est ouverte et cliquer sur  "Download" et finalement "Import"
* Ça devrait être bon, cliquer sur le bouton play en haut au milieu de l'écran pour lancer le jeu

Pour plus de détails : https://medium.com/@linojon/git-and-unity-getting-started-ad7c42be8324
Pour le workflow de contribution : https://www.atlassian.com/git/tutorials/comparing-workflows/feature-branch-workflow

# Multiplayer testing

* Ctrl+B pour build le jeu, ça lance le jeu en version standalone
    *  Peut être convenient de forcer le jeu à rouler en windowed : "Edit" => "Project settings" => "Player" => "Resolution and Presentation"
* Lancer le jeu dans Unity pour avoir une deuxième version qui roule
* Cliquer sur start dans l'une des applications
* Ensuite cliquer sur "Client" dans l'autre application



# Code conventions

## Naming
* fields use camelCase
* methods use PascalCase
* properties (using get/set) use PascalCase
* classes used for UI are suffixed with "UI"

## Structure
* More needed... Its a bit hectic right now
* Structs and enums are in their own file
* Utility file for static functions that aren't really proper to any one class

## Common patterns
### Networking
* use Mirror SyncVars (or other Sync*) to store all gamestate (see GAMESTATE.txt file here)
* Syncvars are modified server side only. Clients can use Mirror Commands to set those syncvars. Alternatively Server only objects (marked as such in their Network Identity component) can directly modify them. 
* Any updates that should be made apparent to clients (UI, sprites, etc) should be done in callbacks hooked into relevant syncvars when those are modified
* Remember to update syncvars or UI when clients join or exit