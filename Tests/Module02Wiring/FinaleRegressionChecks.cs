using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Core.Quiz.Domain;
using GameData.Quiz;
using Modules.Module02_RackInstallation.Domain;
using Systems.Save;

static class FinaleRegressionChecks
{
    private static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    public static int Run()
    {
        int count=0;
        void Check(bool condition, string message) { count++; if (!condition) throw new Exception(message); }
        var completion = new Module02CompletionState();
        Check(!completion.CompleteQuiz(), "No completar módulo antes de la práctica");
        Check(completion.CompletePractice() && !completion.QuizCompleted, "Práctica no equivale a quiz");
        Check(!completion.CompletePractice(), "Práctica idempotente");
        Check(completion.CompleteQuiz() && completion.QuizCompleted, "Entrega completa el módulo");
        Check(!completion.CompleteQuiz(), "Completitud idempotente entre reintentos");
        Check(!new Module02CompletionState().PracticeCompleted, "Una sesión nueva reinicia la práctica");
        var data=new QuizData(); Set(data,"quizId","test");
        var question=new QuizQuestion(); Set(question,"statement","Pregunta");
        Set(question,"options",new List<string>{"A","B"}); Set(question,"correctOptionIndex",1);
        Set(data,"questions",new List<QuizQuestion>{question,question});
        var quiz=new QuizSession(data);
        Check(!quiz.AreAllQuestionsAnswered,"Inicio sin respuestas");
        bool blocked=false; try { quiz.CalculateResult(); } catch (InvalidOperationException) { blocked=true; }
        Check(blocked,"No entregar sin responder todo");
        quiz.SelectAnswer(0,0); quiz.SelectAnswer(1,0);
        Check(quiz.AreAllQuestionsAnswered && !quiz.CalculateResult().Passed,"Entrega con puntuación baja válida");
        quiz.SelectAnswer(0,1); quiz.SelectAnswer(1,1);
        Check(quiz.CalculateResult().Percentage == 100f,"Editar respuestas recalcula resultado");
        Check(!new QuizSession(data).AreAllQuestionsAnswered,"Repetir quiz limpia las respuestas");
        bool invalid=false; try { quiz.SelectAnswer(0,2); } catch (ArgumentOutOfRangeException) { invalid=true; }
        Check(invalid,"Rechazar opciones fuera de rango");

        // Archivos temporales propios; no utiliza partidas reales ni Application.persistentDataPath.
        string path=Path.Combine(Path.GetTempPath(),"module02-save-test-"+Guid.NewGuid().ToString("N")+".json");
        try
        {
            AtomicLocalFile.Write(path,"primero");
            Check(File.ReadAllText(path)=="primero","Primer guardado atómico");
            AtomicLocalFile.Write(path,"segundo");
            Check(File.ReadAllText(path)=="segundo","Reemplazo atómico");
            bool failed=false;
            try { AtomicLocalFile.Write(Path.Combine(path,"invalid.json"),"no"); }
            catch (IOException) { failed=true; } catch (UnauthorizedAccessException) { failed=true; }
            Check(failed && File.ReadAllText(path)=="segundo","Fallo no altera el guardado existente");
        }
        finally { if (File.Exists(path)) File.Delete(path); }
        return count;
    }
}
