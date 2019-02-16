﻿using UnityEngine;
using System.Collections;
using System.Linq;

namespace CBG {
    public class RuntimeFunctionTest : MonoBehaviour {

        public void TestRewinderGetHitboxes() {
            var rewinders = FindObjectsOfType<NetRewinder>();
            foreach (var rewinder in rewinders) {
                rewinder.GetHitboxes();
                var previewer = rewinder.GetComponent<RewindPreviewer>();
                previewer.Cleanup();
                previewer.Init();
            }
        }

        public void TestRewinderSetHitboxes() {
            var rewinders = FindObjectsOfType<NetRewinder>();
            foreach (var rewinder in rewinders) {
                var hitboxes = rewinder.hitBoxes.ToList();
                hitboxes.RemoveAt(Random.Range(0, hitboxes.Count));
                rewinder.SetHitboxes(hitboxes);
                var previewer = rewinder.GetComponent<RewindPreviewer>();
                previewer.Cleanup();
                previewer.Init();
            }
        }
    }
}


