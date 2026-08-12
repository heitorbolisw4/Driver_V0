# Documento de Regras de Negócio — Driver App
## Escopo do Projeto

*Levantado a partir de entrevista com o cliente, do Documento Descritivo do Projeto, da Proposta de Produto e do diagrama de classes atual do backend.*

Agosto de 2026 — heitorbolisw4@gmail.com

---

## 1. Introdução e Objetivo

O Driver App é um sistema voltado a motoristas autônomos que fecham fretes com empresas contratantes com base em quilometragem prevista. Na prática, a quilometragem rodada dentro das cidades (coleta, entrega, deslocamentos) frequentemente ultrapassa a margem combinada no acordo, e esse excedente não é coberto pela contratante — o motorista arca com a diferença sem ter hoje uma forma objetiva de medir esse impacto.

Objetivo do projeto: oferecer ao motorista maior controle sobre os gastos de cada rota e um indicador objetivo — o percentual de aproveitamento — que mostra se o acordo fechado com a contratante foi realmente vantajoso.

Este documento formaliza as regras de negócio que sustentam esse objetivo, delimitando o escopo desta fase do projeto e servindo de referência única entre produto e desenvolvimento. As decisões aqui registradas vêm de entrevista direta com o cliente; pontos ainda sem definição estão sinalizados na Seção 8.

---

## 2. Escopo do Projeto

### 2.1 Dentro do escopo (MVP)

- Cadastro de Caminhão (Truck)
- Registro de Rota (Route), com abertura e encerramento
- Registro de Carga (TruckLoad), múltiplas cargas por rota
- Registro de Gastos por rota, com categorias customizáveis pelo motorista (Expense / ExpenseCategory — entidade nova, ver Seção 4.5)
- Cálculo automático do percentual de aproveitamento da rota
- Histórico de leitura de hodômetro (OdometerReading), gerado no encerramento de cada rota
- Edição e exclusão de registros já cadastrados
- Regra de alerta quando o aproveitamento cai abaixo de um limite definido pelo motorista

### 2.2 Fora do escopo nesta fase

- Gestão de frota por transportadora — não há papel de gestor/admin nem hierarquia de motoristas; cada conta é individual e isolada
- Checklist formal de "Conferência do Estado do Caminhão" — funcionalidade citada no material de produto, mas sem regras de negócio definidas nesta versão
- Leituras intermediárias de hodômetro durante o trajeto — só há leitura no encerramento da rota
- Status de cancelamento de rota — o modelo mantém apenas Em andamento / Concluída
- Canal de entrega do alerta (push, e-mail, banner) — definida apenas a condição de disparo, não o meio
- Autenticação avançada (recuperação de senha, múltiplos perfis de permissão) — o campo Password existe na entidade Driver, mas regras detalhadas de autenticação não fazem parte deste documento

---

## 3. Atores

Motorista Autônomo (Driver) é o único ator do sistema nesta versão. Cada conta é individual e isolada: um motorista não visualiza nem edita dados cadastrados por outro motorista. Não existe papel de administrador, gestor de frota ou transportadora nesta fase.

---

## 4. Entidades e Regras de Negócio

### 4.1 Driver (Motorista)

| Atributo | Descrição |
|---|---|
| Id | Identificador único |
| Name / Email / Password | Dados de cadastro e login |
| IsActive | Indica se a conta está ativa |
| AlertThreshold (novo) | Percentual mínimo de aproveitamento aceitável, definido pelo próprio motorista |

- **RN-01** — O e-mail deve ser único no sistema e funciona como identificador de login.
- **RN-02** — Um motorista só acessa e edita os recursos (Truck, Route, TruckLoad, Expense, OdometerReading) que ele mesmo cadastrou.
- **RN-03** — AlertThreshold é definido pelo próprio motorista e usado para disparar o alerta de aproveitamento (ver RN-24). Valor padrão sugerido: 100%.
- **RN-04** — Um motorista com IsActive = false não consegue autenticar nem operar no sistema.

### 4.2 Truck (Caminhão)

| Atributo | Descrição |
|---|---|
| Id / DriverId | Identificador e vínculo com o motorista dono |
| Plate | Placa do veículo |
| Hodometer | Quilometragem atual do caminhão |

- **RN-05** — A placa deve ser única por motorista. Não há verificação entre contas diferentes nesta fase (ver Pontos em Aberto).
- **RN-06** — O hodômetro inicial informado no cadastro deve ser maior ou igual a zero.
- **RN-07** — O hodômetro do caminhão é atualizado automaticamente ao encerrar uma rota (RN-13); não é editável diretamente pelo motorista.
- **RN-08** — Um caminhão só pode estar vinculado a uma rota em andamento por vez. Um motorista com mais de um caminhão pode, portanto, ter mais de uma rota em andamento simultaneamente — uma por veículo.

### 4.3 Route (Rota)

| Atributo | Descrição |
|---|---|
| Id / DriverId / TruckId | Identificador e vínculos |
| RouteName | Nome/identificação da rota |
| Kilometers | Km acordado com a contratante |
| KilometersCovered | Km rodado, calculado no encerramento |
| StartDate / EndDate | Datas de abertura e encerramento |
| InProcess | Indica se a rota está em andamento |

- **RN-09** — A abertura de uma rota exige um caminhão cadastrado e pertencente ao motorista, e Km Acordado maior que zero.
- **RN-10** — Ao abrir a rota, o hodômetro de abertura é copiado automaticamente do valor atual de Truck.Hodometer — o motorista não digita esse valor novamente.
- **RN-11** — Não é permitido abrir uma nova rota para um caminhão que já possua outra rota com InProcess = true (trava por caminhão, conforme RN-08).
- **RN-12** — O encerramento da rota exige a leitura final do hodômetro e o total de gastos da rota.
- **RN-13** — Km Rodado (KilometersCovered) = leitura final do hodômetro − hodômetro de abertura da rota. A rota não pode ser encerrada se o resultado for menor ou igual a zero.
- **RN-14** — A rota permanece editável — inclusive dados, gastos e cargas — mesmo após concluída. Não há trava de imutabilidade nesta versão (decisão do cliente; ver ressalva na Seção 8).
- **RN-15** — Não existe status de cancelamento nesta versão. O modelo mantém apenas os estados Em andamento e Concluída (ver Seção 7).

### 4.4 TruckLoad (Carga)

| Atributo | Descrição |
|---|---|
| Id / RouteId | Identificador e vínculo com a rota |
| Type | Tipo da carga |
| Weight | Peso da carga |
| IsShipped | Indica se a carga foi entregue/enviada |

- **RN-16** — Uma rota pode ter zero ou mais cargas associadas (relação 0..* entre Route e TruckLoad).
- **RN-17** — Quando informado, o peso (Weight) deve ser maior que zero.

### 4.5 Expense e ExpenseCategory (Gasto) — entidade proposta

Nem o Documento Descritivo nem o diagrama de classes atual modelam o gasto em valores monetários — Route não tem campo de valor. Como a proposta de produto e o cálculo de aproveitamento fazem referência a um "gasto final" e o cliente definiu que as categorias de gasto devem ser customizáveis pelo motorista, este documento propõe duas entidades novas:

| Entidade | Atributos | Descrição |
|---|---|---|
| ExpenseCategory | Id, DriverId, Name | Categoria de gasto criada pelo próprio motorista (sugestão de categorias iniciais pré-cadastradas: Combustível, Alimentação, Entrega/Pedágio) |
| Expense | Id, RouteId, CategoryId, Value, Note | Um lançamento de gasto vinculado a uma rota e a uma categoria |

- **RN-18** — As categorias de gasto são específicas de cada motorista — não são compartilhadas entre contas diferentes.
- **RN-19** — O Gasto Real de uma rota é a soma de todos os lançamentos de Expense vinculados a ela.
- **RN-20** — O valor (Value) de cada lançamento de gasto deve ser maior ou igual a zero.

### 4.6 OdometerReading (Leitura do Hodômetro)

| Atributo | Descrição |
|---|---|
| Id / TruckId | Identificador e vínculo com o caminhão |
| Value | Valor da leitura |
| ReadingDate | Data da leitura |
| Note | Observação |

- **RN-21** — Uma leitura é criada automaticamente ao encerrar cada rota, com o valor final informado pelo motorista, associada ao caminhão da rota.
- **RN-22** — Não há leitura intermediária nesta versão — o histórico reflete apenas os encerramentos de rota (decisão do cliente).
- **RN-23** — O valor de uma nova leitura deve ser maior ou igual à última leitura registrada para aquele caminhão, preservando a consistência cronológica do histórico.

> ⚠️ RN-23 é uma regra proposta por boa prática de consistência de dados — não foi explicitamente confirmada na entrevista. Ver Seção 8.

---

## 5. Regra de Cálculo — Percentual de Aproveitamento da Rota

> **% Aproveitamento = (Km Acordado ÷ Km Rodado) × 100**

- Km Acordado = Route.Kilometers
- Km Rodado = Route.KilometersCovered, calculado conforme RN-13
- Resultado ≥ 100%: o motorista teve margem de sobra no acordo urbano combinado com a contratante.
- Resultado < 100%: o motorista excedeu a margem combinada — é o "dinheiro deixado na mesa" que motiva o projeto.
- A divisão por zero é impedida pela RN-13, que não permite encerrar uma rota com Km Rodado igual a zero ou negativo.

> ⚠️ **Inconsistência a resolver com o cliente:** o Documento Descritivo afirma que o percentual é "comparado ao gasto final informado pelo motorista", mas a fórmula publicada usa exclusivamente quilometragem, sem nenhuma variável monetária. Até definição em contrário, este documento assume que o Gasto Real (Seção 4.5) é exibido junto ao percentual apenas como informação de apoio, e **não** entra no cálculo do indicador. Se o cliente quiser um indicador financeiro combinado (ex.: Custo por Km, ou Aproveitamento Financeiro = Valor Acordado ÷ Gasto Real), é necessária uma segunda métrica — hoje fora de escopo.

---

## 6. Regra de Alerta

- **RN-24** — Ao encerrar uma rota, se o percentual de aproveitamento for menor que Driver.AlertThreshold, o sistema sinaliza a rota como "abaixo do esperado".
- **RN-25** — O canal de entrega do alerta (notificação push, e-mail, indicador visual na tela) não está definido nesta versão. Está no escopo apenas a condição de disparo (RN-24), não o meio de entrega.

---

## 7. Fluxo de Estados da Rota

O modelo mantém o campo booleano `InProcess` do diagrama de classes original, sem introduzir um enum de status mais granular nesta fase:

```
Em andamento  →  Concluída
```

Não existe estado de cancelamento nesta versão (RN-15). Uma rota aberta segue até ser encerrada; se o motorista desistir dela, o registro permanece com InProcess = true indefinidamente — ponto a revisitar (Seção 8).

---

## 8. Pontos em Aberto — a validar com o cliente

- Unicidade de placa entre contas de motoristas diferentes (RN-05) — hoje não há verificação cross-conta.
- RN-14 (rota editável após concluída): recomendação técnica é travar a edição pós-conclusão para preservar a confiabilidade do histórico usado em negociação futura com a contratante. A decisão atual do cliente foi manter editável — revisitar se o histórico passar a ser usado como evidência em negociação.
- RN-23 (consistência cronológica do hodômetro): confirmar se deve haver bloqueio (rejeitar leitura menor que a anterior) ou apenas um alerta não bloqueante.
- Composição final das categorias de gasto: confirmar se Combustível, Alimentação e Entrega/Pedágio vêm pré-cadastradas por padrão para todo motorista novo, ou se a lista começa vazia.
- Papel do Gasto Real na fórmula de aproveitamento (Seção 5) — inconsistência entre os dois documentos de origem sobre se o gasto entra ou não no cálculo.
- Checklist de "Conferência do Estado do Caminhão": funcionalidade citada no material de produto e no diagrama de casos de uso, mas sem regras de negócio definidas — requer levantamento futuro.
- Valor padrão de AlertThreshold (RN-24): sugerido 100%, pendente de confirmação do cliente.
- Regras de exclusão: este documento cobre que exclusão está dentro do escopo (Seção 2.1), mas não define se um caminhão com rotas vinculadas pode ser excluído, nem o que ocorre com cargas e gastos de uma rota excluída — requer definição antes da implementação.

---

## 9. Fontes

- Driver_App_Documento_Descritivo.pdf (Agosto/2026)
- Driver_App_Proposta_de_Produto.pdf (Agosto/2026)
- Diagrama de classes e diagrama de caso de uso do projeto (doc/DriveV0_Classes.drawio.png, doc/UseCase.drawio.png)
- Código-fonte atual do backend — pasta Entities/ (Driver.cs, Truck.cs, Route.cs, TruckLoad.cs, OdometerReading.cs)
- Entrevista de levantamento de regras de negócio realizada com o cliente em agosto de 2026
