using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Home;

public sealed record PublicHomeCopy(
    string PageTitle,
    string HeroHeading,
    string HeroDescription,
    string GetStarted,
    string AlreadyHaveAccount,
    string ContinueToBeeDay,
    string StepsEyebrow,
    string StepsHeading,
    string StepsDescription,
    string Step1Title,
    string Step1Description,
    string Step2Title,
    string Step2Description,
    string Step3Title,
    string Step3Description)
{
    public static PublicHomeCopy For(UserLanguage language) => language switch
    {
        UserLanguage.Portuguese => Portuguese,
        _ => English
    };

    private static readonly PublicHomeCopy English = new(
        PageTitle: "BeeDay — Be better every day",
        HeroHeading: "Build a better day, one step at a time.",
        HeroDescription: "Keep your habits, tasks, and personal progress together.",
        GetStarted: "Get started",
        AlreadyHaveAccount: "I already have an account",
        ContinueToBeeDay: "Continue to BeeDay",
        StepsEyebrow: "A SIMPLE DAILY LOOP",
        StepsHeading: "How BeeDay works",
        StepsDescription: "Start with what matters, turn it into action, and let consistency build the result.",
        Step1Title: "Define",
        Step1Description: "Choose what you want to improve.",
        Step2Title: "Practice",
        Step2Description: "Keep realistic daily actions in view.",
        Step3Title: "Evolve",
        Step3Description: "See completed work become progress.");

    private static readonly PublicHomeCopy Portuguese = new(
        PageTitle: "BeeDay — Seja melhor a cada dia",
        HeroHeading: "Construa um dia melhor, um passo de cada vez.",
        HeroDescription: "Mantenha seus hábitos, tarefas e progresso pessoal em um só lugar.",
        GetStarted: "Comece agora",
        AlreadyHaveAccount: "Já tenho uma conta",
        ContinueToBeeDay: "Continuar no BeeDay",
        StepsEyebrow: "UM CICLO DIÁRIO SIMPLES",
        StepsHeading: "Como o BeeDay funciona",
        StepsDescription: "Comece pelo que importa, transforme em ação e deixe a consistência construir o resultado.",
        Step1Title: "Definir",
        Step1Description: "Escolha o que você quer melhorar.",
        Step2Title: "Praticar",
        Step2Description: "Mantenha ações diárias realistas à vista.",
        Step3Title: "Evoluir",
        Step3Description: "Veja o trabalho concluído virar progresso.");
}
