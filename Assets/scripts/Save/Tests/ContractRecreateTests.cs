using NUnit.Framework;
using UnityEngine;

namespace CrystalFlux.SaveSystem.Tests
{
    public class ContractRecreateTests
    {
        [Test]
        public void RecreateInstance_ReproducesRolledValues()
        {
            var amd = ScriptableObject.CreateInstance<AnomalyData>();
            amd.anomalyType = AnomalyType.Swarm;
            amd.anomalyMinVal = 1f;
            amd.anomalyMaxVal = 1000f;

            Random.InitState(1234);
            var original = (SwarmInstance)amd.CreateInstance();

            Random.InitState(999);
            var again = (SwarmInstance)amd.RecreateInstance(original.creationState);

            Assert.AreEqual(original.penaltyAmount, again.penaltyAmount);
            Assert.AreEqual(original.Description, again.Description);

            Object.DestroyImmediate(amd);
        }

        [Test]
        public void RecreateInstance_LeavesGlobalRandomUntouched()
        {
            var amd = ScriptableObject.CreateInstance<AnomalyData>();
            amd.anomalyType = AnomalyType.Swarm;
            var state = amd.CreateInstance().creationState;

            Random.InitState(42);
            Random.State before = Random.state;
            amd.RecreateInstance(state);
            Assert.AreEqual(JsonUtility.ToJson(before), JsonUtility.ToJson(Random.state));

            Object.DestroyImmediate(amd);
        }
    }
}
