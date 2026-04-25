using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Mahan.Assets.Scripts.Tests
{
    public class ManMonsterPlayModeTests
    {
        private GameObject ground;
        private GameObject monsterObject;
        private GameObject playerObject;

        private ManMonster monster;
        private NavMeshAgent agent;
        private AudioSource musicSource;
        private AudioSource footstepSource;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            NavMesh.RemoveAllNavMeshData();

            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Test Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f);

            NavMeshSurface surface = ground.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.BuildNavMesh();

            monsterObject = new GameObject("Monster");
            monsterObject.transform.position = Vector3.zero;

            agent = monsterObject.AddComponent<NavMeshAgent>();
            agent.radius = 0.25f;
            agent.height = 2f;
            agent.speed = 2.5f;
            agent.acceleration = 20f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0f;

            musicSource = monsterObject.AddComponent<AudioSource>();
            footstepSource = monsterObject.AddComponent<AudioSource>();

            monster = monsterObject.AddComponent<ManMonster>();

            playerObject = new GameObject("Player");
            playerObject.transform.position = new Vector3(4f, 0f, 4f);

            monster.player = playerObject.transform;
            monster.agent = agent;
            monster.visualRoot = monsterObject.transform;
            monster.musicSource = musicSource;
            monster.footstepSource = footstepSource;

            monster.patrolSpeed = 2.5f;
            monster.slowChaseSpeed = 3.5f;
            monster.fastChaseSpeed = 5.5f;
            monster.runAwaySpeed = 7f;

            monster.patrolRadius = 5f;
            monster.patrolWaitTime = 0.05f;
            monster.patrolPointReachDistance = 1.2f;

            monster.runPointReachDistance = 10f;
            monster.vanishDelay = 0.05f;

            monster.patrolLoop = CreateSilentClip("Patrol Loop");
            monster.slowChaseLoop = CreateSilentClip("Slow Chase Loop");
            monster.fastChaseLoop = CreateSilentClip("Fast Chase Loop");

            monster.patrolFootsteps = CreateSilentClip("Patrol Footsteps");
            monster.slowChaseFootsteps = CreateSilentClip("Slow Chase Footsteps");
            monster.fastChaseFootsteps = CreateSilentClip("Fast Chase Footsteps");

            yield return null;
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            NavMesh.RemoveAllNavMeshData();

            if (monsterObject != null)
                Object.DestroyImmediate(monsterObject);

            if (playerObject != null)
                Object.DestroyImmediate(playerObject);

            if (ground != null)
                Object.DestroyImmediate(ground);
        }

        [UnityTest]
        public IEnumerator Start_SetsMonsterToPatrol()
        {
            yield return null;

            Assert.AreEqual(ManMonster.MonsterState.Patrol, GetCurrentState());
            Assert.AreEqual(monster.patrolSpeed, agent.speed, 0.01f);
            Assert.AreEqual(monster.patrolLoop, musicSource.clip);
            Assert.AreEqual(monster.patrolFootsteps, footstepSource.clip);
        }

        [UnityTest]
        public IEnumerator ChangeState_ToSlowChase_UpdatesSpeedMusicAndFootsteps()
        {
            InvokeChangeState(ManMonster.MonsterState.ChaseSlow);

            yield return null;

            Assert.AreEqual(ManMonster.MonsterState.ChaseSlow, GetCurrentState());
            Assert.AreEqual(monster.slowChaseSpeed, agent.speed, 0.01f);
            Assert.AreEqual(monster.slowChaseLoop, musicSource.clip);
            Assert.AreEqual(monster.slowChaseFootsteps, footstepSource.clip);
        }

        [UnityTest]
        public IEnumerator ChangeState_ToFastChase_UpdatesSpeedMusicAndFootsteps()
        {
            InvokeChangeState(ManMonster.MonsterState.ChaseFast);

            yield return null;

            Assert.AreEqual(ManMonster.MonsterState.ChaseFast, GetCurrentState());
            Assert.AreEqual(monster.fastChaseSpeed, agent.speed, 0.01f);
            Assert.AreEqual(monster.fastChaseLoop, musicSource.clip);
            Assert.AreEqual(monster.fastChaseFootsteps, footstepSource.clip);
        }

        [UnityTest]
        public IEnumerator ChangeState_BackToPatrol_UpdatesSpeedMusicAndFootsteps()
        {
            InvokeChangeState(ManMonster.MonsterState.ChaseFast);
            yield return null;

            InvokeChangeState(ManMonster.MonsterState.Patrol);
            yield return null;

            Assert.AreEqual(ManMonster.MonsterState.Patrol, GetCurrentState());
            Assert.AreEqual(monster.patrolSpeed, agent.speed, 0.01f);
            Assert.AreEqual(monster.patrolLoop, musicSource.clip);
            Assert.AreEqual(monster.patrolFootsteps, footstepSource.clip);
        }

        [UnityTest]
        public IEnumerator RunToPointAndVanish_DisablesMonster()
        {
            GameObject target = new GameObject("Run Target");
            target.transform.position = new Vector3(1f, 0f, 1f);

            monster.TriggerRunToPointAndVanish(target.transform);

            yield return null;
            yield return new WaitForSeconds(monster.vanishDelay + 0.1f);

            Assert.AreEqual(ManMonster.MonsterState.RunToPointAndVanish, GetCurrentState());
            Assert.AreEqual(monster.runAwaySpeed, agent.speed, 0.01f);
            Assert.IsFalse(monsterObject.activeSelf);

            Object.DestroyImmediate(target);
        }

        [UnityTest]
        public IEnumerator RunToPointAndVanish_WithNullTarget_DoesNothing()
        {
            monster.TriggerRunToPointAndVanish(null);

            yield return null;

            Assert.AreEqual(ManMonster.MonsterState.Patrol, GetCurrentState());
            Assert.IsTrue(monsterObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator TriggerRunToPointAndVanish_OnlyWorksOnce()
        {
            GameObject firstTarget = new GameObject("First Target");
            firstTarget.transform.position = new Vector3(1f, 0f, 1f);

            GameObject secondTarget = new GameObject("Second Target");
            secondTarget.transform.position = new Vector3(3f, 0f, 3f);

            monster.TriggerRunToPointAndVanish(firstTarget.transform);
            monster.TriggerRunToPointAndVanish(secondTarget.transform);

            yield return null;

            Assert.AreEqual(ManMonster.MonsterState.RunToPointAndVanish, GetCurrentState());
            Assert.AreEqual(monster.runAwaySpeed, agent.speed, 0.01f);

            Object.DestroyImmediate(firstTarget);
            Object.DestroyImmediate(secondTarget);
        }

        private ManMonster.MonsterState GetCurrentState()
        {
            FieldInfo field = typeof(ManMonster).GetField(
                "currentState",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(field);

            return (ManMonster.MonsterState)field.GetValue(monster);
        }

        private void InvokeChangeState(ManMonster.MonsterState state)
        {
            MethodInfo method = typeof(ManMonster).GetMethod(
                "ChangeState",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(method);

            method.Invoke(monster, new object[] { state });
        }

        private static AudioClip CreateSilentClip(string name)
        {
            return AudioClip.Create(
                name,
                4410,
                1,
                44100,
                false
            );
        }
    }
}