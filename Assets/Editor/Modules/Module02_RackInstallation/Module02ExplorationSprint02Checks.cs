#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Modules.Module02_RackInstallation.Data;
using Modules.Module02_RackInstallation.Exploration;
using Modules.Module02_RackInstallation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Prueba de lógica aislada: no guarda escenas, datos ni progreso.</summary>
public static class Module02ExplorationSprint02Checks
{
    [MenuItem("Network Simulator/Module 02/Sprint 2 - Comprobar lógica sin visor")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Ejecuta esta prueba fuera de Play Mode.");
        var root = new GameObject("Sprint02Checks") { hideFlags = HideFlags.HideAndDontSave };
        var data = ScriptableObject.CreateInstance<RackComponentInfo>();
        try
        {
            Set(data, "sections", new List<RackInfoSection> { new() { title = "Prueba", body = "Contenido" } });
            var a = Child(root, "A").AddComponent<RackInfoTarget>();
            var b = Child(root, "B").AddComponent<RackInfoTarget>();
            var alias = Child(root, "Alias").AddComponent<RackInfoTarget>();
            Set(a, "information", data); Set(b, "information", data); Set(alias, "parentTarget", a);
            var focus = root.AddComponent<InfoFocusDetector>();
            object left = new(), right = new();
            int requests = 0;
            focus.CardRequested += _ => requests++;

            focus.BeginFocus(a, left);
            Invoke(focus, "Update");
            Require(requests == 0, "No debe abrir antes del dwell.");
            Set(focus, "focusStartedAt", Time.unscaledTime - 2f);
            Invoke(focus, "Update"); Invoke(focus, "Update");
            Require(requests == 1, "Dwell dispara una sola solicitud.");
            focus.BeginFocus(a, right);
            focus.EndFocus(a, left);
            Require(focus.CurrentTarget == a, "Salir con una mano no cancela la otra.");
            focus.BeginFocus(b, left);
            focus.EndFocus(a, left);
            Require(focus.CurrentTarget == b, "Exit tardío no cancela un hover nuevo.");
            focus.EndFocus(b, left);
            Require(focus.CurrentTarget == a, "Regresa al puntero restante.");
            focus.BeginFocus(alias, left);
            Require(focus.CurrentTarget == a && alias.Information == data, "Collider hijo comparte tarjeta.");
            focus.EndFocus(a, right); focus.EndFocus(alias, left);
            Require(focus.CurrentTarget == null, "Sin punteros se limpia el enfoque.");

            var view = Child(root, "View").AddComponent<InfoCardView>();
            var pointer = Child(root, "Pointer").AddComponent<InfoCardPointerArea>();
            Set(view, "pointerArea", pointer);
            var controller = root.AddComponent<InfoCardController>();
            Set(controller, "focusDetector", focus); Set(controller, "view", view);
            Invoke(controller, "HandleCardRequested", a);
            Require(controller.State == InfoCardState.Open, "Solicitud abre tarjeta.");
            controller.TogglePin();
            Invoke(controller, "HandleCardRequested", b);
            Require(controller.State == InfoCardState.Pinned && Get<RackInfoTarget>(controller, "displayedTarget") == a,
                "Fijar conserva tarjeta al apuntar a otro objeto.");
            controller.NextPage(); controller.NextPage();
            Require(Get<int>(controller, "page") == 1, "No exceder la última página.");
            controller.PreviousPage(); controller.PreviousPage();
            Require(Get<int>(controller, "page") == 0, "No navegar antes de la primera página.");
            controller.TogglePin();
            Require(controller.State == InfoCardState.Open, "Liberar restaura cierre automático.");
            pointer.OnPointerEnter(new PointerEventData(null) { pointerId = 1 });
            Set(controller, "closeAt", Time.unscaledTime - 10f);
            Invoke(controller, "Update");
            Require(controller.State == InfoCardState.Open, "Apuntar al Canvas mantiene tarjeta abierta.");
            pointer.OnPointerExit(new PointerEventData(null) { pointerId = 1 });
            Set(controller, "closeAt", Time.unscaledTime - 10f);
            Invoke(controller, "Update");
            Require(controller.State == InfoCardState.Hidden, "Salir y agotar el margen cierra la tarjeta.");
            focus.BeginFocus(a, left);
            controller.Close();
            Require(Get<bool>(focus, "requested"), "Cerrar impide reapertura inmediata.");
            Debug.Log("Sprint 2: comprobaciones de dwell, dos punteros, parent target, pin, páginas y cierre superadas.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(data);
        }
    }

    private static GameObject Child(GameObject root, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(root.transform);
        return child;
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Sprint 2: " + message);
    }
    private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
    private static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
    private static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    private static void Invoke(object target, string method, params object[] arguments) =>
        target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, arguments);
}
#endif
