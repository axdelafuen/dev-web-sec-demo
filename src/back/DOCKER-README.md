# DevWebSecDemo - Guide Docker

## Prérequis

- Docker Desktop installé et en cours d'exécution
- Docker Compose installé

## Démarrage de l'application

### Option 1 : Avec Docker Compose (Recommandé)

Cette méthode démarre automatiquement PostgreSQL et l'API :

```powershell
# Depuis le dossier src/back
docker-compose up --build
```

L'API sera accessible sur : http://localhost:8080
Swagger UI sera accessible sur : http://localhost:8080/swagger

### Option 2 : Build et run manuel

```powershell
# Build de l'image
docker build -t devwebsecdemo-api -f DevWebSecDemo.WebAPI/Dockerfile .

# Run du conteneur
docker run -p 8080:8080 devwebsecdemo-api
```

## Arrêt de l'application

```powershell
# Arrêt des services
docker-compose down

# Arrêt et suppression des volumes (efface la base de données)
docker-compose down -v
```

## Base de données

La base de données PostgreSQL est configurée avec :
- **Host** : postgres (dans Docker) / localhost (en local)
- **Port** : 5432
- **Database** : devwebsecdemo
- **Username** : devwebsecdemo
- **Password** : devwebsecdemo@123

Les migrations Entity Framework Core sont appliquées automatiquement au démarrage de l'API en mode Production.

## Configuration JWT

Les tokens JWT sont configurés avec :
- **Issuer** : dev-web-sec-demo
- **Audience** : admin
- **SecretKey** : (configuré dans docker-compose.yml)

## Logs

Pour voir les logs en temps réel :

```powershell
# Logs de tous les services
docker-compose logs -f

# Logs de l'API uniquement
docker-compose logs -f api

# Logs de PostgreSQL uniquement
docker-compose logs -f postgres
```

## Troubleshooting

### L'API ne démarre pas
1. Vérifiez que PostgreSQL est démarré : `docker-compose ps`
2. Vérifiez les logs : `docker-compose logs api`
3. Vérifiez que le port 8080 n'est pas déjà utilisé

### Erreur de connexion à la base de données
1. Vérifiez que PostgreSQL est healthy : `docker-compose ps`
2. Attendez quelques secondes que PostgreSQL soit complètement démarré
3. Redémarrez l'API : `docker-compose restart api`

### Reset complet
```powershell
docker-compose down -v
docker-compose up --build
```

## Développement local (sans Docker)

Si vous souhaitez développer localement :

1. Démarrez uniquement PostgreSQL avec Docker :
```powershell
docker-compose up postgres
```

2. Lancez l'API depuis Visual Studio ou avec :
```powershell
cd DevWebSecDemo.WebAPI
dotnet run
```

## Migrations Entity Framework Core

Pour créer une nouvelle migration :

```powershell
cd DevWebSecDemo.WebAPI
dotnet ef migrations add NomDeLaMigration
```

Pour appliquer les migrations manuellement :

```powershell
dotnet ef database update
```
