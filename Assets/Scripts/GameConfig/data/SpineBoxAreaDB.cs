/*
 * Generate by EnhanceExcel2Anything,don't modify it!
 * From: 寻宝关卡奖励.xlsx
*/

namespace GameConfig
{
    using System.Collections.Generic;
    using UnityEngine;


    public partial class SpineBoxAreaDB : ConfigBase
    {
        private SpineBoxArea[] _data;
        private Dictionary<string, int> _idToIdx;
        
        protected override void ConstructConfig()
        {
            _data = new SpineBoxArea[]
            {
                new(spineID: "none", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 0.0f, y: 0.0f)),
                new(spineID: "dtxy_A_12", offset: new Vector2(x: 0.0f, y: -3.5f), size: new Vector2(x: 3.0f, y: 2.5f)),
                new(spineID: "dtxy_C_07", offset: new Vector2(x: -0.2f, y: 1.3f), size: new Vector2(x: 2.5f, y: 3.0f)),
                new(spineID: "dtxy_C_16", offset: new Vector2(x: -0.5f, y: -0.3f), size: new Vector2(x: 2.2f, y: 1.6f)),
                new(spineID: "dtxy_C_18", offset: new Vector2(x: -0.6f, y: 2.3f), size: new Vector2(x: 6.0f, y: 2.8f)),
                new(spineID: "dtxy_D_05", offset: new Vector2(x: -3.1f, y: 2.0f), size: new Vector2(x: 2.3f, y: 3.0f)),
                new(spineID: "dtxy_B_10", offset: new Vector2(x: -0.5f, y: -0.2f), size: new Vector2(x: 2.0f, y: 2.0f)),
                new(spineID: "dtxy_C_15", offset: new Vector2(x: 0.2f, y: 0.0f), size: new Vector2(x: 1.0f, y: 2.0f)),
                new(spineID: "dtxy_A_06", offset: new Vector2(x: 0.26f, y: 0.0f), size: new Vector2(x: 1.72f, y: 1.18f)),
                new(spineID: "dtxy_B_03", offset: new Vector2(x: 0.37f, y: 0.0f), size: new Vector2(x: 2.36f, y: 2.31f)),
                new(spineID: "dtxy_A_05", offset: new Vector2(x: -0.11f, y: -0.5f), size: new Vector2(x: 2.08f, y: 1.8f)),
                new(spineID: "gwds_C_11", offset: new Vector2(x: 0.17f, y: 0.83f), size: new Vector2(x: 5.41f, y: 1.81f)),
                new(spineID: "gwds_D_04", offset: new Vector2(x: -0.09f, y: 0.0f), size: new Vector2(x: 1.12f, y: 1.91f)),
                new(spineID: "gwds_B_08", offset: new Vector2(x: 0.12f, y: 0.0f), size: new Vector2(x: 1.48f, y: 2.16f)),
                new(spineID: "gwds_C_09", offset: new Vector2(x: -0.05f, y: -0.22f), size: new Vector2(x: 1.94f, y: 1.19f)),
                new(spineID: "gwds_D_5", offset: new Vector2(x: -1.01f, y: 0.71f), size: new Vector2(x: 3.5f, y: 1.86f)),
                new(spineID: "gwds_B_05", offset: new Vector2(x: -1.1f, y: -1.31f), size: new Vector2(x: 2.8f, y: 3.47f)),
                new(spineID: "gwds_D_19", offset: new Vector2(x: 0.59f, y: 0.13f), size: new Vector2(x: 0.71f, y: 0.93f)),
                new(spineID: "gwds_D_15", offset: new Vector2(x: 0.78f, y: -0.65f), size: new Vector2(x: 2.86f, y: 2.42f)),
                new(spineID: "gwds_D_9", offset: new Vector2(x: -8.48f, y: -1.6f), size: new Vector2(x: 3.65f, y: 3.15f)),
                new(spineID: "gwds_D_12", offset: new Vector2(x: 0.1f, y: 0.06f), size: new Vector2(x: 2.64f, y: 3.01f)),
                new(spineID: "gwds_B_04", offset: new Vector2(x: -0.45f, y: -0.26f), size: new Vector2(x: 0.64f, y: 0.73f)),
                new(spineID: "gwds_B_12", offset: new Vector2(x: 0.34f, y: 1.96f), size: new Vector2(x: 7.02f, y: 4.09f)),
                new(spineID: "gwds_D_13", offset: new Vector2(x: -0.19f, y: -0.25f), size: new Vector2(x: 2.74f, y: 2.46f)),
                new(spineID: "gwds_B_02", offset: new Vector2(x: -0.24f, y: 0.19f), size: new Vector2(x: 1.88f, y: 3.92f)),
                new(spineID: "gwds_D_08", offset: new Vector2(x: -0.27f, y: 0.07f), size: new Vector2(x: 1.18f, y: 1.44f)),
                new(spineID: "gwds_B_14", offset: new Vector2(x: -0.05f, y: -0.14f), size: new Vector2(x: 5.17f, y: 1.15f)),
                new(spineID: "gwds_A_09", offset: new Vector2(x: -0.75f, y: 0.14f), size: new Vector2(x: 2.93f, y: 2.43f)),
                new(spineID: "gwds_A_08", offset: new Vector2(x: 0.76f, y: 0.45f), size: new Vector2(x: 1.4f, y: 0.77f)),
                new(spineID: "gwds_A_20", offset: new Vector2(x: 0.08f, y: -0.78f), size: new Vector2(x: 2.51f, y: 1.06f)),
                new(spineID: "gwds_A_10", offset: new Vector2(x: -1.94f, y: 0.76f), size: new Vector2(x: 0.71f, y: 0.66f)),
                new(spineID: "gwds_C_06", offset: new Vector2(x: 0.0f, y: -0.3f), size: new Vector2(x: 5.12f, y: 2.97f)),
                new(spineID: "gwds_C_08", offset: new Vector2(x: 0.0f, y: 0.15f), size: new Vector2(x: 2.18f, y: 1.47f)),
                new(spineID: "gwds_C_07", offset: new Vector2(x: 0.35f, y: 0.07f), size: new Vector2(x: 2.84f, y: 1.38f)),
                new(spineID: "gwds_C_12", offset: new Vector2(x: -0.71f, y: -0.69f), size: new Vector2(x: 2.59f, y: 1.0f)),
                new(spineID: "gwds_D_03", offset: new Vector2(x: 0.08f, y: 0.59f), size: new Vector2(x: 2.65f, y: 1.65f)),
                new(spineID: "gwds_B_15", offset: new Vector2(x: -0.48f, y: -0.11f), size: new Vector2(x: 2.57f, y: 1.73f)),
                new(spineID: "gwds_B_19", offset: new Vector2(x: 0.18f, y: -0.5f), size: new Vector2(x: 2.45f, y: 1.22f)),
                new(spineID: "gwds_B_16", offset: new Vector2(x: 0.78f, y: 1.29f), size: new Vector2(x: 2.08f, y: 2.06f)),
                new(spineID: "gwds_A_18", offset: new Vector2(x: 0.49f, y: 0.43f), size: new Vector2(x: 5.06f, y: 1.82f)),
                new(spineID: "gwds_A_15", offset: new Vector2(x: -0.07f, y: 0.0f), size: new Vector2(x: 3.0f, y: 2.1f)),
                new(spineID: "gwds_A_02", offset: new Vector2(x: 0.17f, y: -0.54f), size: new Vector2(x: 7.22f, y: 2.33f)),
                new(spineID: "gwds_B_10", offset: new Vector2(x: -1.24f, y: -3.24f), size: new Vector2(x: 0.66f, y: 1.2f)),
                new(spineID: "gwds_C_17", offset: new Vector2(x: -0.27f, y: -0.34f), size: new Vector2(x: 1.0f, y: 1.09f)),
                new(spineID: "gwds_A_06", offset: new Vector2(x: -1.81f, y: 0.78f), size: new Vector2(x: 4.49f, y: 1.2f)),
                new(spineID: "gwds_A_14", offset: new Vector2(x: 0.16f, y: -0.25f), size: new Vector2(x: 3.44f, y: 2.0f)),
                new(spineID: "gwds_C_16", offset: new Vector2(x: 0.11f, y: 0.19f), size: new Vector2(x: 3.81f, y: 5.04f)),
                new(spineID: "gwds_A_05", offset: new Vector2(x: -0.66f, y: -0.67f), size: new Vector2(x: 2.64f, y: 1.82f)),
                new(spineID: "gwds_C_15", offset: new Vector2(x: -0.67f, y: -0.55f), size: new Vector2(x: 1.97f, y: 1.26f)),
                new(spineID: "gwds_D_18", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 0.1f, y: 0.1f)),
                new(spineID: "jlls_A_12", offset: new Vector2(x: 1.76f, y: 0.58f), size: new Vector2(x: 1.8f, y: 3.1f)),
                new(spineID: "jlls_A_11", offset: new Vector2(x: 1.54f, y: 0.0f), size: new Vector2(x: 1.8f, y: 4.1f)),
                new(spineID: "jlls_A_13", offset: new Vector2(x: 0.0f, y: 0.4f), size: new Vector2(x: 3.4f, y: 3.9f)),
                new(spineID: "jlls_B_11", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 4.65f, y: 3.04f)),
                new(spineID: "jlls_D_13", offset: new Vector2(x: 0.47f, y: 0.0f), size: new Vector2(x: 2.68f, y: 2.64f)),
                new(spineID: "jlls_C_04", offset: new Vector2(x: 0.0f, y: -0.6f), size: new Vector2(x: 6.3f, y: 3.23f)),
                new(spineID: "jlls_C_07", offset: new Vector2(x: 0.33f, y: -0.25f), size: new Vector2(x: 1.88f, y: 2.14f)),
                new(spineID: "jlls_C_05", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 3.19f, y: 6.36f)),
                new(spineID: "jlls_C_15", offset: new Vector2(x: 0.0f, y: 0.24f), size: new Vector2(x: 2.8f, y: 1.59f)),
                new(spineID: "pdlxq_B_15", offset: new Vector2(x: -0.06f, y: 0.46f), size: new Vector2(x: 3.07f, y: 1.73f)),
                new(spineID: "pdlxq_B_11", offset: new Vector2(x: -0.2f, y: 0.51f), size: new Vector2(x: 3.38f, y: 2.58f)),
                new(spineID: "pdlxq_A_17", offset: new Vector2(x: 0.0f, y: -0.19f), size: new Vector2(x: 2.63f, y: 2.58f)),
                new(spineID: "pdlxq_A_11", offset: new Vector2(x: 0.38f, y: 0.21f), size: new Vector2(x: 0.83f, y: 4.84f)),
                new(spineID: "pdlxq_A_07", offset: new Vector2(x: 0.38f, y: -0.15f), size: new Vector2(x: 2.39f, y: 3.6f)),
                new(spineID: "pdlxq_A_20", offset: new Vector2(x: 1.33f, y: -0.05f), size: new Vector2(x: 2.39f, y: 3.6f)),
                new(spineID: "pdlxq_A_15", offset: new Vector2(x: -0.18f, y: 0.13f), size: new Vector2(x: 2.71f, y: 1.74f)),
                new(spineID: "pdlxq_A_16", offset: new Vector2(x: 2.89f, y: -0.31f), size: new Vector2(x: 2.51f, y: 2.82f)),
                new(spineID: "pdlxq_B_04", offset: new Vector2(x: 0.2f, y: 0.09f), size: new Vector2(x: 1.99f, y: 2.38f)),
                new(spineID: "pdlxq_B_16", offset: new Vector2(x: 0.2f, y: -0.87f), size: new Vector2(x: 3.83f, y: 3.84f)),
                new(spineID: "pdlxq_D_06", offset: new Vector2(x: -0.38f, y: -0.25f), size: new Vector2(x: 4.66f, y: 3.66f)),
                new(spineID: "pdlxq_D_08", offset: new Vector2(x: 0.01f, y: 1.63f), size: new Vector2(x: 0.74f, y: 0.63f)),
                new(spineID: "pdlxq_D_07", offset: new Vector2(x: -0.01f, y: -0.32f), size: new Vector2(x: 2.07f, y: 1.72f)),
                new(spineID: "pdlxq_D_18", offset: new Vector2(x: -1.23f, y: 0.0f), size: new Vector2(x: 0.55f, y: 0.63f)),
                new(spineID: "pdlxq_C_16", offset: new Vector2(x: -0.78f, y: 0.4f), size: new Vector2(x: 2.28f, y: 2.5f)),
                new(spineID: "pdlxq_C_18", offset: new Vector2(x: -0.48f, y: 0.0f), size: new Vector2(x: 2.35f, y: 2.26f)),
                new(spineID: "pdlxq_C_04", offset: new Vector2(x: 0.77f, y: 0.0f), size: new Vector2(x: 2.12f, y: 8.21f)),
                new(spineID: "jqryrgw_D_03", offset: new Vector2(x: -0.6f, y: 0.0f), size: new Vector2(x: 0.82f, y: 0.63f)),
                new(spineID: "jqryrgw_D_07", offset: new Vector2(x: 0.0f, y: -0.56f), size: new Vector2(x: 0.64f, y: 0.73f)),
                new(spineID: "jqryrgw_D_05", offset: new Vector2(x: 0.27f, y: 0.0f), size: new Vector2(x: 5.04f, y: 6.4f)),
                new(spineID: "jqryrgw_D_19", offset: new Vector2(x: 0.0f, y: 0.15f), size: new Vector2(x: 1.4f, y: 1.24f)),
                new(spineID: "jqryrgw_D_18", offset: new Vector2(x: -0.18f, y: 0.0f), size: new Vector2(x: 0.86f, y: 0.62f)),
                new(spineID: "jqryrgw_C_04", offset: new Vector2(x: 0.0f, y: 0.11f), size: new Vector2(x: 7.37f, y: 4.02f)),
                new(spineID: "jqryrgw_A_03", offset: new Vector2(x: 0.01f, y: -0.83f), size: new Vector2(x: 8.65f, y: 2.74f)),
                new(spineID: "jqryrgw_B_05", offset: new Vector2(x: -0.54f, y: 0.61f), size: new Vector2(x: 1.35f, y: 1.8f)),
                new(spineID: "jqryrgw_A_06", offset: new Vector2(x: -0.93f, y: -0.96f), size: new Vector2(x: 2.15f, y: 2.02f)),
                new(spineID: "jqryrgw_A_02", offset: new Vector2(x: -0.55f, y: -0.59f), size: new Vector2(x: 1.06f, y: 1.19f)),
                new(spineID: "jqryrgw_B_07", offset: new Vector2(x: 0.03f, y: 0.15f), size: new Vector2(x: 1.92f, y: 1.69f)),
                new(spineID: "jqryrgw_B_08", offset: new Vector2(x: 0.95f, y: 0.27f), size: new Vector2(x: 1.66f, y: 2.12f)),
                new(spineID: "jqryrgw_B_18", offset: new Vector2(x: 1.01f, y: -1.95f), size: new Vector2(x: 5.38f, y: 2.38f)),
                new(spineID: "jqryrgw_B_17", offset: new Vector2(x: 0.26f, y: 0.48f), size: new Vector2(x: 2.43f, y: 3.93f)),
                new(spineID: "jqryrgw_B_19", offset: new Vector2(x: -0.36f, y: 0.28f), size: new Vector2(x: 2.81f, y: 5.48f)),
                new(spineID: "nhsbzz_a_06", offset: new Vector2(x: -1.8f, y: 0.21f), size: new Vector2(x: 5.45f, y: 7.23f)),
                new(spineID: "nhsbzz_b_14", offset: new Vector2(x: 0.0f, y: 0.48f), size: new Vector2(x: 4.33f, y: 3.52f)),
                new(spineID: "fwrj_A_16", offset: new Vector2(x: -5.46f, y: 1.07f), size: new Vector2(x: 4.88f, y: 2.09f)),
                new(spineID: "fwrj_B_12", offset: new Vector2(x: 0.0f, y: 0.3f), size: new Vector2(x: 3.36f, y: 3.0f)),
                new(spineID: "tmgf _C_03", offset: new Vector2(x: -0.24f, y: -0.36f), size: new Vector2(x: 0.93f, y: 2.81f)),
                new(spineID: "tmgf _C_02", offset: new Vector2(x: 0.0f, y: 0.53f), size: new Vector2(x: 4.08f, y: 3.21f)),
                new(spineID: "tmgf _C_05", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 1.78f, y: 3.65f)),
                new(spineID: "tmgf _C_08", offset: new Vector2(x: -1.56f, y: 0.53f), size: new Vector2(x: 3.44f, y: 2.86f)),
                new(spineID: "tmgf _D_10", offset: new Vector2(x: 0.0f, y: 0.18f), size: new Vector2(x: 4.51f, y: 5.93f)),
                new(spineID: "tmgf _B_17", offset: new Vector2(x: 0.26f, y: -0.96f), size: new Vector2(x: 1.46f, y: 1.25f)),
                new(spineID: "tmgf _B_18", offset: new Vector2(x: 0.1f, y: 0.57f), size: new Vector2(x: 1.37f, y: 1.34f)),
                new(spineID: "tmgf _B_07", offset: new Vector2(x: -2.01f, y: -2.76f), size: new Vector2(x: 2.4f, y: 3.87f)),
                new(spineID: "tmgf _B_08", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 3.17f, y: 3.17f)),
                new(spineID: "tmgf _D_04", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 4.25f, y: 4.25f)),
                new(spineID: "tmgf _D_02", offset: new Vector2(x: 1.06f, y: 0.0f), size: new Vector2(x: 2.86f, y: 1.82f)),
                new(spineID: "tmgf _D_08", offset: new Vector2(x: 0.0f, y: 0.6f), size: new Vector2(x: 3.65f, y: 1.77f)),
                new(spineID: "tmgf _D_01", offset: new Vector2(x: 0.67f, y: 0.21f), size: new Vector2(x: 4.15f, y: 2.69f)),
                new(spineID: "tmgf _B_16", offset: new Vector2(x: 0.0f, y: 0.22f), size: new Vector2(x: 4.76f, y: 1.37f)),
                new(spineID: "sscltt_A_02", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 2.96f, y: 2.6f)),
                new(spineID: "sscltt_A_09", offset: new Vector2(x: 1.52f, y: -0.47f), size: new Vector2(x: 2.46f, y: 4.93f)),
                new(spineID: "sscltt_B_31", offset: new Vector2(x: 2.55f, y: -0.36f), size: new Vector2(x: 2.54f, y: 3.78f)),
                new(spineID: "sscltt_D_64", offset: new Vector2(x: 0.0f, y: 0.66f), size: new Vector2(x: 2.35f, y: 1.95f)),
                new(spineID: "sscltt_D_63", offset: new Vector2(x: 0.0f, y: 2.28f), size: new Vector2(x: 4.86f, y: 2.79f)),
                new(spineID: "sscltt_D_65", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 1.31f, y: 2.57f)),
                new(spineID: "sscltt_C_60", offset: new Vector2(x: -0.04f, y: -1.09f), size: new Vector2(x: 3.08f, y: 3.99f)),
                new(spineID: "sscltt_D_62", offset: new Vector2(x: -1.17f, y: -3.63f), size: new Vector2(x: 2.06f, y: 3.88f)),
                new(spineID: "dhtt_D_12", offset: new Vector2(x: -0.21f, y: 0.13f), size: new Vector2(x: 2.77f, y: 1.17f)),
                new(spineID: "dhtt_D_17", offset: new Vector2(x: 0.28f, y: -0.13f), size: new Vector2(x: 2.26f, y: 1.76f)),
                new(spineID: "dhtt_B_15", offset: new Vector2(x: 0.3f, y: 0.1f), size: new Vector2(x: 2.83f, y: 4.7f)),
                new(spineID: "dhtt_B_14", offset: new Vector2(x: -0.65f, y: 0.37f), size: new Vector2(x: 2.94f, y: 3.28f)),
                new(spineID: "dhtt_B_19", offset: new Vector2(x: -0.1f, y: 0.27f), size: new Vector2(x: 2.12f, y: 2.23f)),
                new(spineID: "dhtt_B_20", offset: new Vector2(x: 0.0f, y: -0.17f), size: new Vector2(x: 1.63f, y: 2.39f)),
                new(spineID: "dhtt_A_20", offset: new Vector2(x: 0.0f, y: 0.96f), size: new Vector2(x: 4.65f, y: 5.51f)),
                new(spineID: "dhtt_A_19", offset: new Vector2(x: -0.84f, y: 0.0f), size: new Vector2(x: 1.62f, y: 4.23f)),
                new(spineID: "dhtt_A_01", offset: new Vector2(x: 0.0f, y: 0.27f), size: new Vector2(x: 5.92f, y: 5.98f)),
                new(spineID: "yyjn_B_05", offset: new Vector2(x: 0.15f, y: 0.22f), size: new Vector2(x: 3.85f, y: 6.35f)),
                new(spineID: "yyjn_A_11", offset: new Vector2(x: 1.22f, y: -1.08f), size: new Vector2(x: 1.57f, y: 1.15f)),
                new(spineID: "yyjn_A_13", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 5.47f, y: 4.48f)),
                new(spineID: "yyjn_A_10", offset: new Vector2(x: 3.39f, y: 3.75f), size: new Vector2(x: 0.71f, y: 1.05f)),
                new(spineID: "yyjn_C_07", offset: new Vector2(x: 6.2f, y: 2.39f), size: new Vector2(x: 2.19f, y: 2.23f)),
                new(spineID: "yyjn_C_20", offset: new Vector2(x: 4.23f, y: 0.48f), size: new Vector2(x: 3.4f, y: 6.91f)),
                new(spineID: "yyjn_E_11", offset: new Vector2(x: 0.0f, y: 0.0f), size: new Vector2(x: 5.04f, y: 3.74f)),
                new(spineID: "jsds_B_01", offset: new Vector2(x: 0.03f, y: 0.0f), size: new Vector2(x: 1.68f, y: 1.95f)),
                new(spineID: "jsds_E_03", offset: new Vector2(x: -0.18f, y: 0.1f), size: new Vector2(x: 2.36f, y: 2.21f)),
                new(spineID: "jsds_D_08", offset: new Vector2(x: -0.12f, y: -0.39f), size: new Vector2(x: 2.35f, y: 2.22f)),
                new(spineID: "jsds_D_09", offset: new Vector2(x: 0.02f, y: -0.24f), size: new Vector2(x: 3.55f, y: 3.49f)),
                new(spineID: "jsds_E_35", offset: new Vector2(x: -2.42f, y: -1.29f), size: new Vector2(x: 2.25f, y: 1.83f)),
                new(spineID: "jsds_C_08", offset: new Vector2(x: -0.3f, y: 0.03f), size: new Vector2(x: 1.8f, y: 3.77f)),
                new(spineID: "jsds_E_22", offset: new Vector2(x: -0.9f, y: 0.32f), size: new Vector2(x: 4.67f, y: 4.0f)),
                new(spineID: "jsds_C_17", offset: new Vector2(x: 0.45f, y: 0.54f), size: new Vector2(x: 2.93f, y: 3.09f)),
                new(spineID: "jsds_C_04", offset: new Vector2(x: -0.35f, y: 0.45f), size: new Vector2(x: 4.2f, y: 3.28f)),
                new(spineID: "jsds_E_18", offset: new Vector2(x: 0.22f, y: 0.79f), size: new Vector2(x: 2.44f, y: 2.35f)),
                new(spineID: "jsds_E_21", offset: new Vector2(x: 0.12f, y: 1.02f), size: new Vector2(x: 3.05f, y: 4.51f)),
                new(spineID: "jsds_C_05", offset: new Vector2(x: 0.79f, y: 0.33f), size: new Vector2(x: 5.17f, y: 2.34f)),
                new(spineID: "jsds_A_06", offset: new Vector2(x: -0.46f, y: -0.1f), size: new Vector2(x: 2.07f, y: 1.36f)),
                new(spineID: "jsds_A_10", offset: new Vector2(x: -0.29f, y: -0.31f), size: new Vector2(x: 1.29f, y: 2.28f)),
                new(spineID: "jsds_E_25", offset: new Vector2(x: -0.5f, y: 0.29f), size: new Vector2(x: 3.74f, y: 1.9f))
            };
            
            MakeIdToIdx();
        }
        
        public ref readonly SpineBoxArea this[string spineID]
        {
            get
            {
                TackUsage();
                var ok = _idToIdx.TryGetValue(spineID, out int idx);
                if (!ok)
                    UnityEngine.Debug.LogError($"[SpineBoxArea] spineID: {spineID} not found");
                return ref _data[idx];
            }
        }
        
        public SpineBoxArea[] All => _data;
        
        public int Count => _data.Length;
        
        public override void Dispose()
        {
            _data = null;
            OnDispose();
        }
        
        private void MakeIdToIdx()
        {
            _idToIdx = new Dictionary<string,int>(_data.Length);
            for (int i = 0; i < _data.Length; i++)
            {
                _idToIdx[_data[i].spineID] = i;
            }
        }
    }
}