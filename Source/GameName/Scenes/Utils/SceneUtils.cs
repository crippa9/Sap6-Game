﻿using Thengill;
using Thengill.Components;
using Thengill.Components.Renderable;
using Thengill.Systems;
using GameName.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thengill.Shaders;

namespace GameName.Scenes.Utils {
    class SceneUtils {
        private static Random rnd = new Random();

        public static int CreateBall(Vector3 p, Vector3 v, float r = 1.0f) {
            var currentScene = Game1.Inst.Scene;
            var ball = currentScene.AddEntity();

            currentScene.AddComponent(ball, new CBody {
                Aabb = new BoundingBox(-r * Vector3.One, r * Vector3.One),
                Radius = r,
                InvMass = 0.1f,
                LinDrag = 0.2f,
                Velocity = v,
                Restitution = 0.3f
            });

            currentScene.AddComponent(ball, new CTransform {
                Position = p,
                Rotation = Matrix.Identity,
                Scale = r * Vector3.One
            });
            currentScene.AddComponent(ball, new CSyncObject());
            currentScene.AddComponent<C3DRenderable>(ball, new CImportedModel {
                model = Game1.Inst.Content.Load<Model>("Models/badboll"),
                fileName = "badboll"
            });
            currentScene.AddComponent(ball, new CPickUp());
            return ball;
        }

        public static Func<float, Matrix> wiggleAnimation(int id)
        {
            var randt = (float)rnd.NextDouble() * 2.0f * MathHelper.Pi;
            var currentScene = Game1.Inst.Scene;
            Func<float, Matrix> npcAnim = (t) => {
                var transf = (CTransform)currentScene.GetComponentFromEntity<CTransform>(id);
                var body = (CBody)currentScene.GetComponentFromEntity<CBody>(id);

                // Wiggle wiggle!
                var x = 0.3f * Vector3.Dot(transf.Frame.Forward, body.Velocity);
                var walk =
                    Matrix.CreateFromAxisAngle(Vector3.Forward, x * 0.1f * (float)Math.Cos(randt + t * 12.0f))
                  * Matrix.CreateTranslation(Vector3.Up * -x * 0.1f * (float)Math.Sin(randt + t * 24.0f));

                var idle = Matrix.CreateTranslation(Vector3.Up * 0.07f * (float)Math.Sin(randt + t * 2.0f));

                return walk * idle;
            };
            return npcAnim;
        }


        public static void CreateAnimals(int numFlocks,int worldsize) {
            var currentScene = Game1.Inst.Scene;

            int membersPerFlock = (int)(rnd.NextDouble() * 10) + 10;
            var flockRadius = membersPerFlock;
            for (int f = 0; f < numFlocks; f++) {
                int flockId = currentScene.AddEntity();
                CFlock flock = new CFlock {
                    Radius = 20,
                    SeparationDistance = 3,
                    AlignmentFactor = 0.1f,
                    CohesionFactor = 0.5f,
                    SeparationFactor = 100.0f,
                    PreferredMovementSpeed = 150f
                };

                double animal = rnd.NextDouble();
                string flockAnimal = animal > 0.66 ? "flossy" : animal > 0.33 ? "goose" : "hen";

                // TODO: get the global value of the worldsize
                int flockX = (int)(rnd.NextDouble() * worldsize);
                int flockZ = (int)(rnd.NextDouble() * worldsize);
                CTransform flockTransform = new CTransform { Position = new Vector3(flockX, 0, flockZ) };
                flockTransform.Position += new Vector3(-worldsize, 0, -worldsize);

                for (int i = 0; i < membersPerFlock; i++) {
                    int id = currentScene.AddEntity();
                    var npcAnim = wiggleAnimation(id);

                    if (flockAnimal.Equals("hen")) {
                        // TODO: Make animals have different animations based on state
                        CAnimation normalAnimation = new CHenNormalAnimation { animFn = npcAnim };
                        // Set a random offset to animation so not all animals are synced
                        normalAnimation.CurrentKeyframe = rnd.Next(normalAnimation.Keyframes.Count - 1);
                        // Random animation speed between 0.8-1.0
                        normalAnimation.AnimationSpeed = (float)rnd.NextDouble() * 0.2f + 0.8f;
                        currentScene.AddComponent<C3DRenderable>(id, normalAnimation);
                    } else {
                        CImportedModel modelComponent = new CImportedModel { animFn = npcAnim };
                        modelComponent.fileName = flockAnimal;
                        modelComponent.model = Game1.Inst.Content.Load<Model>("Models/" + modelComponent.fileName);
                        currentScene.AddComponent<C3DRenderable>(id, modelComponent);
                    }

                    float memberX = flockTransform.Position.X + (float)rnd.NextDouble() * flockRadius * 2 - flockRadius;
                    float memberZ = flockTransform.Position.Z + (float)rnd.NextDouble() * flockRadius * 2 - flockRadius;
                    float y = flockTransform.Position.Y;
                    CTransform transformComponent = new CTransform();

                    transformComponent.Position = new Vector3(memberX, y, memberZ);
                    transformComponent.Rotation = Matrix.CreateFromAxisAngle(Vector3.UnitY,
                        (float)(Math.PI * (rnd.NextDouble() * 2)));
                    float size = 0.5f;
                    transformComponent.Scale = new Vector3(1f);
                    currentScene.AddComponent(id, transformComponent);
                    currentScene.AddComponent(id, new CBody {
                        InvMass = 0.05f,
                        Aabb = new BoundingBox(-size * Vector3.One, size * Vector3.One),
                        LinDrag = 0.8f,
                        Velocity = Vector3.Zero,
                        Radius = 1f,
                        SpeedMultiplier = size,
                        MaxVelocity = 4,
                        Restitution = 0
                    });
                    // health value of npcs, maybe change per species/flock/member?
                    var npcHealth = 1;
                    currentScene.AddComponent(id, new CHealth { MaxHealth = npcHealth, Health = npcHealth });
                    currentScene.AddComponent(id, new CAI { Flock = flockId });
                    currentScene.AddComponent(id, new CSyncObject());

                    flock.Members.Add(id);
                }
                currentScene.AddComponent(flockId, flock);
                currentScene.AddComponent(flockId, flockTransform);
            }
        }

        public static void CreateCollectables(int numPowerUps, int worldsize) {
            var currentScene = Game1.Inst.Scene;

            // TODO: get the global value of the worldsize
            int chests = numPowerUps, hearts = numPowerUps;
            for (int i = 0; i < chests; i++) {
                var id = currentScene.AddEntity();
                currentScene.AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "chest", model = Game1.Inst.Content.Load<Model>("Models/chest") });
                var z = (float)(rnd.NextDouble() * worldsize);
                var x = (float)(rnd.NextDouble() * worldsize);
                var chestScale = 0.25f;
                currentScene.AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(chestScale) });
                currentScene.AddComponent(id, new CBody() { Aabb = new BoundingBox(-chestScale * Vector3.One, chestScale * Vector3.One) });
                currentScene.AddComponent(id, new CSyncObject());
            }
            for (int i = 0; i < hearts; i++) {
                var id = currentScene.AddEntity();
                currentScene.AddComponent<C3DRenderable>(id, new CImportedModel { fileName = "heart", model = Game1.Inst.Content.Load<Model>("Models/heart") });
                var z = (float)(rnd.NextDouble() * worldsize);
                var x = (float)(rnd.NextDouble() * worldsize);
                currentScene.AddComponent(id, new CTransform { Position = new Vector3(x, -50, z), Scale = new Vector3(1f) });
                currentScene.AddComponent(id, new CBody() { Aabb = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 1)) });
                currentScene.AddComponent(id, new CSyncObject());
            }
        }

		public static void SpawnEnvironment(Heightmap heightmap, int worldsize)
		{
			Dictionary<int, string> elementList = new Dictionary<int, string>();
			elementList.Add(255, "LeafTree");
			elementList.Add(245, "PalmTree");
			elementList.Add(235, "tree");
			elementList.Add(170, "rock2");

                        var matDic = new Dictionary<int, MaterialShader>();
                        matDic = null;
                        // TODO: Models are not properly vertex colored so code below makes
                        //       everything blue.
                        // var toonMat = new ToonMaterial(Vector3.One*0.2f,
                        //                                new Vector3(1.0f, 0.0f, 1.0f), // ignored
                        //                                Vector3.Zero,
                        //                                40.0f,
                        //                                null, // diftex
                        //                                null, // normtex
                        //                                1.0f, // normcoeff
                        //                                5, // diflevels
                        //                                2, // spelevels,
                        //                                true); // use vert col


                        // for (var i = 0; i < 20; i++) {
                        //     matDic[i] = toonMat;
                        // }

			for (int y = 0; y<heightmap.GetDimensions().Y; y++)
			{
				for (int x = 0; x<heightmap.GetDimensions().X; x++)
				{
					if (elementList.ContainsKey(heightmap.ColorAt(x, y).B))
					{
						int newElement = Game1.Inst.Scene.AddEntity();
						string type = elementList[(int)heightmap.ColorAt(x, y).B];
						var wx = (x / heightmap.GetDimensions().X - 0.5f) * worldsize;
						var wy = (y / heightmap.GetDimensions().Y - 0.5f) * worldsize;
						//Game1.Inst.Scene.AddComponent(newElement, new CBox() { Box = new BoundingBox(new Vector3(-1, -5, -1), new Vector3(1, 5, 1)), InvTransf = Matrix.Identity });
						Game1.Inst.Scene.AddComponent(newElement, new CTransform() { Position = new Vector3(worldsize * (x / (float)heightmap.GetDimensions().X - 0.5f), heightmap.HeightAt(wx, wy) - 1.5f, worldsize * (y / (float)heightmap.GetDimensions().Y - 0.5f)), Scale = new Vector3((float)rnd.NextDouble() * 0.25f + 0.75f), Rotation = Matrix.CreateRotationY((float)rnd.NextDouble() * MathHelper.Pi * 2f) });
						Game1.Inst.Scene.AddComponent<C3DRenderable>(newElement, new CImportedModel() { model = Game1.Inst.Content.Load<Model>("Models/" + type), fileName = type, materials = matDic, enableVertexColor = false });
					}
				}
			}
		}

        public static void CreateTriggerEvents(int numTriggers, int worldsize) {
            var currentScene = Game1.Inst.Scene;
            Random rnd = new Random();

            // TODO: get the global value of the worldsize
            for (int i = 0; i < numTriggers; i++) {
                int id = currentScene.AddEntity();
                var z = (float)(rnd.NextDouble() * worldsize);
                var x = (float)(rnd.NextDouble() * worldsize);
                currentScene.AddComponent(id, new CBody() { Radius = 5, Aabb = new BoundingBox(new Vector3(-5, -5, -5), new Vector3(5, 5, 5)), LinDrag = 0.8f });
                currentScene.AddComponent(id, new CTransform() { Position = new Vector3(x, -50, z),Scale = new Vector3(1f) });
                if (rnd.NextDouble() > 0.5) {
                    // Falling balls event
                    currentScene.OnEvent("collision", data => {
                        foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>().Keys)
                        {
                            if ((((PhysicsSystem.CollisionInfo) data).Entity1 == player &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == id)
                                ||
                                (((PhysicsSystem.CollisionInfo) data).Entity1 == id &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == player))
                            {
                                CTransform playerPosition =
                                    (CTransform) currentScene.GetComponentFromEntity<CTransform>(player);
                                for (var j = 0; j < 6; j++)
                                {
                                    var r = 0.6f + (float) rnd.NextDouble() * 2.0f;
                                    Utils.SceneUtils.CreateBall(
                                        new Vector3((float) Math.Sin(j) * j + playerPosition.Position.X,
                                                    playerPosition.Position.Y + 10f + 2.0f * j,
                                                    (float) Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                        new Vector3(0.0f, -50.0f, 0.0f), // Velocity
                                        r); // Radius
                                    //balls.Add(ballId);
                                }
                            }
                        }
                    });
                } else {
                    // Balls spawns around the player
                    currentScene.OnEvent("collision", data => {
                        foreach (var player in Game1.Inst.Scene.GetComponents<CPlayer>().Keys)
                        {
                            if ((((PhysicsSystem.CollisionInfo) data).Entity1 == player &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == id)
                                ||
                                (((PhysicsSystem.CollisionInfo) data).Entity1 == id &&
                                 ((PhysicsSystem.CollisionInfo) data).Entity2 == player))
                            {
                                CTransform playerPosition =
                                    (CTransform) currentScene.GetComponentFromEntity<CTransform>(player);
                                for (var j = 0; j < 6; j++)
                                {
                                    var r = 0.6f + (float) rnd.NextDouble() * 2.0f;
                                    Utils.SceneUtils.CreateBall(
                                        new Vector3((float) Math.Sin(j) * j + playerPosition.Position.X,
                                            playerPosition.Position.Y + 2f,
                                            (float) Math.Cos(j) * j + playerPosition.Position.Z), // Position
                                        new Vector3(0.0f, 0.0f, 0.0f), // Velocity
                                            r); // Radius
                                    //balls.Add(ballId);
                                }
                            }
                        }
                    });
                }
            }
        }
    }
}
