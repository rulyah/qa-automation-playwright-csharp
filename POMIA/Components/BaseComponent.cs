using Microsoft.Playwright;

namespace PlaywrightTests.POMIA.Components;

/// <summary>
/// Базовий компонент: шматок UI, який повторюється на багатьох сторінках (хедер, кошик, форма логіну).
/// Усі внутрішні локатори шукаються всередині <see cref="Root"/>, а не по всій сторінці —
/// саме це рятує від "збитих локаторів", коли схожа розмітка зустрічається двічі.
/// </summary>
public abstract class BaseComponent
{
    protected IPage Page { get; }

    /// <summary>Кореневий елемент компонента — межа пошуку для всіх дочірніх локаторів.</summary>
    protected ILocator Root { get; }

    protected BaseComponent(IPage page, ILocator root)
    {
        Page = page;
        Root = root;
    }
}
