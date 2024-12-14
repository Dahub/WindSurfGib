# WindSurf Gibraltar

Application de gestion d'inventaire pour WindSurf Gibraltar, composée de trois services principaux :
- Une interface publique pour la consultation
- Une interface d'administration pour la gestion des données
- Une API backend pour le traitement des données

## Architecture

Le projet est structuré en trois applications principales :

```
WindSurfGib/
├── src/
│   ├── frontend/     # Interface publique (Vue.js)
│   ├── admin/        # Interface d'administration (Vue.js)
│   └── backend/      # API backend (ASP.NET Core)
└── docker-compose.yml
```

## Prérequis

- Docker et Docker Compose
- .NET 8.0+ (pour le développement backend)
- Node.js 16+ (pour le développement frontend)

## Installation et Lancement

1. Cloner le repository :
```bash
git clone [URL_DU_REPO]
cd WindSurfGib
```

2. Lancer l'application avec Docker Compose :
```bash
docker-compose up --build
```

L'application sera accessible aux adresses suivantes :
- Frontend : http://localhost:8080
- Admin : http://localhost:8081
- Backend : http://localhost:5269

## Développement Local

### Frontend et Admin
```bash
cd src/frontend  # ou cd src/admin
npm install
npm run dev
```

### Backend
```bash
cd src/backend
dotnet restore
dotnet run
```

## Format du Fichier CSV

Le système attend un fichier CSV avec les spécifications suivantes :

- Séparateur : point-virgule (;)
- Encodage : UTF-8
- Extension : .csv

### Structure des colonnes

```csv
Nom Modele Feuille;Code magasin;Nom magasin;Code article;Désignation;Famille;Sous-Famille;Quantite;Reference fournisseur;QuantiteTerrain
EPS PARIS;205;Paris Centre;ART001;Article 1;FAM1;SFAM1;10;REF001;5
```

### Colonnes requises

| Colonne | Description |
|---------|-------------|
| Nom Modele Feuille | Nom de l'agence |
| Code magasin | Code identifiant du magasin |
| Nom magasin | Nom du magasin |
| Code article | Code unique de l'article |
| Désignation | Nom de l'article |
| Famille | Catégorie principale |
| Sous-Famille | Sous-catégorie |
| Quantite | Stock théorique |
| Reference fournisseur | Référence du fournisseur |
| QuantiteTerrain | Stock réel sur le terrain |

## Fonctionnalités

### Interface Publique (Frontend)
- Consultation du catalogue d'équipements par agence et magasin
- Filtrage par famille et sous-famille
- Affichage des quantités théoriques et réelles

### Interface d'Administration (Admin)
- Téléchargement du fichier CSV actuel
- Upload d'un nouveau fichier CSV (fusion avec l'existant)
- Suppression du fichier d'inventaire avec confirmation

### API Backend
- Endpoints REST pour la gestion des données
- Validation des fichiers CSV
- Gestion des erreurs

## Sécurité

- L'interface d'administration doit être accessible uniquement en interne
- Les uploads de fichiers sont limités aux CSV
- Validation du format des fichiers uploadés

## Maintenance

### Sauvegarde
Il est recommandé de :
- Sauvegarder régulièrement le fichier CSV
- Conserver un historique des modifications

### Monitoring
Les logs sont disponibles via :
```bash
docker-compose logs -f
```

## Support

Pour toute question ou problème :
1. Consulter la documentation
2. Vérifier les logs
3. Contacter l'équipe technique

## License

Propriétaire - Tous droits réservés
