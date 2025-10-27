# PIZZA API

Cette solution a pour objectif de mettre en place une API REST monolitique pour la gestion de commandes de pizza en ligne.

On utilise docker pour le déploiement de l'application ASP.NET dans un container. On doit également disposer d'un container supplémentaire pour héberger une base de données MySQL servant au test avec le model relationnel suivant

```mermaid
erDiagram
    CUSTOMER ||--o{ ORDER : places
    ORDER ||--|{ ORDER_ITEM : contains
    PIZZA ||--o{ ORDER_ITEM : includes
    PIZZA_BASE ||--o{ PIZZA : has
    PIZZA_INGREDIENT }o--o{ PIZZA : includes


    CUSTOMER {
        int id
        string first_name
        string last_name
        string email
        string password_hash
        int order_amount
    }
    ORDER {
        int id
        date orderDate
        string status
        float total_price
    }
    PIZZA {
        int id
        string name
        float price
    }
    ORDER_ITEM {
        int quantity
        float price
    }
    PIZZA_BASE {
        int id
        string name
    }
    PIZZA_INGREDIENT{
        int id
        string name
    }
```

Les spécifications de l'API ne sont pas compètes.