using DevoidEngine.Engine.Utilities;
using System.Collections;

public class Coroutine
{
    private IEnumerator enumerator;

    public Coroutine(IEnumerator enumerator)
    {
        this.enumerator = enumerator;
    }

    public bool MoveNext()
    {
        return enumerator.MoveNext();
    }

    public static Coroutine StartCoroutine(IEnumerator enumerator)
    {
        Coroutine coroutine = new Coroutine(enumerator);
        coroutine.MoveNext();
        return coroutine;
    }

    public static IEnumerator WaitForSeconds(float seconds, float dt = 1/60)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < seconds)
        {
            yield return null;
            elapsedTime += dt; // you can replace this with a stopwatch or other timing mechanism
        }
        // code to execute after waiting
    }
}