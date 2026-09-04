using System.Collections.Generic;
using UnityEngine;

namespace Modules.Module02_RackInstallation.Data
{
    [CreateAssetMenu(fileName = "RackComponentInfo", menuName = "Network Simulator/Module 02/Rack Component Info")]
    public sealed class RackComponentInfo : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private RackComponentCategory category;
        [SerializeField] private bool educationalApproximation = true;

        [Header("Educational content")]
        [TextArea(2, 5)] [SerializeField] private string shortDescription;
        [TextArea(2, 5)] [SerializeField] private string function;
        [Min(0)] [SerializeField] private int rackUnits;
        [SerializeField] private Sprite image;

        [Header("Reference (optional)")]
        [SerializeField] private string standardReference;

        [Header("Additional pages (optional; page 1 is the overview)")]
        [SerializeField] private List<RackInfoSection> sections = new();
        public IReadOnlyList<RackInfoSection> Sections => sections;

        public string Id => id;
        public string DisplayName => displayName;
        public RackComponentCategory Category => category;
        public bool EducationalApproximation => educationalApproximation;
        public string ShortDescription => shortDescription;
        public string Function => function;
        public int RackUnits => rackUnits;
        public Sprite Image => image;
        public string StandardReference => standardReference;
    }
}
