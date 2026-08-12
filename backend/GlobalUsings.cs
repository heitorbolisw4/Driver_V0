// O SDK Web expõe Microsoft.AspNetCore.Routing como using global, que já define um
// tipo "Route". Este alias garante que "Route" resolva para a entidade do domínio
// em todo o projeto, evitando erros de referência ambígua (CS0104).
global using Route = backend.Entities.Route;
