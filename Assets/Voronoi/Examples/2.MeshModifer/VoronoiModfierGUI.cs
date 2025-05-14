using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hlwd.Game
{
    public class VoronoiModfierGUI : MonoBehaviour
    {
        public Button saveBtn, pattenChangeBtn;
        public Text pattenText;
        // Start is called before the first frame update
        void Start()
        {
            saveBtn.onClick.AddListener(OnClickSave);
            pattenChangeBtn.onClick.AddListener(OnChangePatten);
        }
        void OnClickSave()
        {
#if UNITY_EDITOR
            VoronoiModifier.Instance.SaveMeshInEditor();
#endif
        }

        void OnChangePatten()
        {
            VoronoiModifier.Instance.ChangeOptPatten();
        }

        private void Update()
        {
            pattenText.text = VoronoiModifier.Instance.ModifyMesh ? "Current Mode: Modify Mesh" : "Current Mode: Modify Vertex";
        }
    }
}
