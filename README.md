# LevelUp

LevelUp é uma aplicação de console desenvolvida em C# que transforma hábitos e atividades pessoais em um sistema de progressão inspirado em jogos de RPG.

Ao concluir hábitos, o jogador recebe pontos de experiência, evolui de nível e acompanha seu progresso. Os dados são armazenados localmente em um arquivo JSON para que o progresso seja mantido entre as execuções do programa.

## Objetivo do projeto

Este projeto foi criado com finalidade educacional para praticar conceitos fundamentais e intermediários de desenvolvimento com C# e .NET, incluindo:

* orientação a objetos;
* classes e objetos;
* propriedades e métodos;
* encapsulamento;
* composição de objetos;
* coleções genéricas;
* estruturas condicionais e de repetição;
* validação de entrada;
* tratamento de valores nulos;
* LINQ e expressões lambda;
* serialização e desserialização JSON;
* leitura e gravação de arquivos;
* separação de responsabilidades;
* injeção de dependência manual.

O projeto também funciona como preparação para estudos futuros de ASP.NET Core, Entity Framework Core, APIs REST e arquitetura de software.

## Funcionalidades atuais

* Criação automática de um personagem;
* Visualização do nome, nível e experiência do personagem;
* Cadastro de hábitos;
* Definição de título, descrição e duração do hábito;
* Cálculo automático da recompensa de experiência;
* Listagem dos hábitos cadastrados;
* Registro da quantidade de conclusões;
* Conclusão de hábitos por identificador;
* Adição de experiência ao personagem;
* Progressão automática de nível;
* Preservação da experiência excedente após subir de nível;
* Salvamento dos dados em JSON;
* Carregamento automático do progresso salvo.

## Tecnologias utilizadas

* C#;
* .NET 10;
* Aplicação de console;
* System.Text.Json;
* LINQ;
* Git;
* GitHub;
* Visual Studio.

## Estrutura do projeto

```text
LevelUp/
├── Models/
│   ├── Character.cs
│   ├── GameData.cs
│   ├── Habit.cs
│   └── PlayerAttributes.cs
├── Services/
│   ├── CharacterService.cs
│   ├── HabitService.cs
│   └── SaveService.cs
├── UI/
│   └── ConsoleMenu.cs
├── Data/
├── Program.cs
└── LevelUp.csproj
```

### Models

A pasta `Models` contém as classes que representam os dados e as entidades do sistema.

#### Character

Representa o personagem do jogador e armazena:

* nome;
* nível;
* experiência atual;
* atributos;
* experiência necessária para o próximo nível.

#### PlayerAttributes

Representa os atributos de RPG do personagem:

* Strength;
* Intelligence;
* Vitality;
* Agility;
* Luck;
* Dexterity.

#### Habit

Representa um hábito cadastrado e armazena:

* identificador;
* título;
* descrição;
* duração em minutos;
* número de conclusões;
* experiência recebida por minuto;
* recompensa total de experiência.

#### GameData

Reúne o personagem e a lista de hábitos em um único objeto para facilitar o salvamento e o carregamento dos dados.

### Services

A pasta `Services` contém as regras de negócio da aplicação.

#### CharacterService

Responsável por:

* criar o personagem;
* adicionar experiência;
* verificar a progressão de nível;
* preservar a experiência excedente.

#### HabitService

Responsável por:

* manter a lista de hábitos;
* cadastrar hábitos;
* listar os hábitos;
* registrar conclusões;
* calcular a recompensa obtida;
* carregar hábitos salvos.

#### SaveService

Responsável por:

* criar a pasta de dados;
* transformar objetos C# em JSON;
* gravar o progresso;
* ler o arquivo salvo;
* reconstruir os objetos da aplicação.

### UI

A pasta `UI` contém a interface da aplicação.

#### ConsoleMenu

Responsável por:

* apresentar o menu principal;
* receber as entradas do usuário;
* validar os valores informados;
* chamar os serviços;
* mostrar os resultados das operações.

## Fluxo da aplicação

```text
Inicialização do programa
        ↓
Criação dos serviços
        ↓
Tentativa de carregar o arquivo salvo
        ↓
Recuperação ou criação do personagem
        ↓
Carregamento dos hábitos
        ↓
Inicialização do menu
        ↓
Usuário seleciona uma operação
        ↓
Execução da regra de negócio
        ↓
Salvamento dos dados ao encerrar
```

## Sistema de experiência

A recompensa de um hábito é calculada com base em sua duração:

```text
Experiência recebida = duração em minutos × experiência por minuto
```

Exemplo:

```text
Duração: 60 minutos
Experiência por minuto: 0,1 XP
Recompensa: 6 XP
```

A experiência necessária para subir de nível é calculada de acordo com o nível atual:

```text
Experiência necessária = nível atual × 100
```

Exemplos:

| Nível atual | Experiência necessária |
| ----------: | ---------------------: |
|           1 |                 100 XP |
|           2 |                 200 XP |
|           3 |                 300 XP |
|           4 |                 400 XP |

Quando o personagem sobe de nível, a experiência excedente é mantida.

## Persistência dos dados

O progresso é armazenado em:

```text
LevelUp/Data/save.json
```

O arquivo contém:

* informações do personagem;
* nível;
* experiência;
* atributos;
* hábitos cadastrados;
* número de conclusões.

O arquivo `save.json` é local e não deve ser enviado ao GitHub, pois representa os dados pessoais de execução de cada usuário.

## Como executar o projeto

### Pré-requisitos

* .NET 10 SDK instalado;
* Visual Studio 2022 ou uma versão compatível;
* Git, caso queira clonar o repositório.

### Clonar o repositório

```bash
git clone https://github.com/tiagoarrigoni/LevelUp.git
```

### Entrar na pasta

```bash
cd LevelUp
```

### Restaurar as dependências

```bash
dotnet restore
```

### Compilar o projeto

```bash
dotnet build
```

### Executar

```bash
dotnet run --project LevelUp/LevelUp.csproj
```

Também é possível abrir o arquivo `LevelUp.slnx` no Visual Studio e executar o projeto por meio da IDE.

## Menu atual

```text
================================
 LEVEL UP
================================

1 - Ver personagem
2 - Cadastrar hábito
3 - Listar hábitos
4 - Concluir hábito
0 - Sair
```

## Próximas melhorias planejadas

* Solicitar o nome do personagem na primeira execução;
* Exibir os atributos do personagem;
* Associar cada hábito a um atributo;
* Aumentar atributos ao concluir atividades;
* Criar categorias de hábitos;
* Implementar tarefas diárias;
* Adicionar sistema de moedas e recompensas;
* Permitir editar e remover hábitos;
* Melhorar a geração de identificadores;
* Salvar automaticamente após alterações;
* Adicionar tratamento de exceções;
* Criar interfaces para os serviços;
* Implementar testes unitários;
* Substituir o JSON por um banco de dados;
* Utilizar Entity Framework Core;
* Transformar o projeto em uma ASP.NET Core Web API;
* Criar uma interface Web para o sistema.

## Conceitos estudados

Durante o desenvolvimento deste projeto estão sendo praticados:

```text
C#
├── Variáveis e tipos
├── Operadores
├── Condicionais
├── Laços de repetição
├── Métodos
├── Classes e objetos
├── Propriedades
├── Encapsulamento
├── Coleções
├── Nullable reference types
├── LINQ
├── Expressões lambda
├── Manipulação de arquivos
└── Serialização JSON
```

## Status do projeto

O projeto está em desenvolvimento e será expandido conforme o avanço dos estudos em C#, .NET e ASP.NET Core.

## Autor

Desenvolvido por **Tiago Arrigoni** como parte de seus estudos em desenvolvimento de software com C# e .NET.
