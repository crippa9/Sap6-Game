﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using EngineName.Components.Renderable;
using EngineName.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Utils;
using EngineName.Components;

namespace EngineName.Systems {
    public class InputSystem : EcsSystem {
        private MapSystem _mapSystem;
        private const float CAMERASPEED = 0.1f;
        private Keys[] lastPressedKeys;
        public InputSystem() { }

        public InputSystem(MapSystem mapSystem) {
            _mapSystem = mapSystem;
        }
       


        public override void Update(float t, float dt){
            KeyboardState currentState = Keyboard.GetState();
            Keys[] pressedKeys = currentState.GetPressedKeys();
            if (currentState.IsKeyDown(Keys.Escape))
                Game1.Inst.Exit();

            foreach (var input in Game1.Inst.Scene.GetComponents<CInput>()) {
                CBody body = null;
                if (Game1.Inst.Scene.EntityHasComponent<CBody>(input.Key)) {
                    body = (CBody)Game1.Inst.Scene.GetComponentFromEntity<CBody>(input.Key);
                }
                if (Game1.Inst.Scene.EntityHasComponent<CCamera>(input.Key)){
                    var transform = (CTransform)Game1.Inst.Scene.GetComponentFromEntity<CTransform>(input.Key);
                    var inputValue = (CInput)input.Value;
                    CCamera cameraComponent = (CCamera)Game1.Inst.Scene.GetComponentFromEntity<CCamera>(input.Key);



                    if (currentState.IsKeyDown(inputValue.CameraMovementForward)){
                        transform.Position     += CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                        cameraComponent.Target += CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                    }           
                    if (currentState.IsKeyDown(inputValue.CameraMovementBackward)){
                        transform.Position     -= CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                        cameraComponent.Target -= CAMERASPEED * new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), 0, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f)));
                    }
                    if (currentState.IsKeyDown(inputValue.CameraMovementLeft)){
                        cameraComponent.Heading -= 0.05f;
                        transform.Position = Vector3.Subtract(cameraComponent.Target, new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), cameraComponent.Height, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f))));
                    }       
                    if (currentState.IsKeyDown(inputValue.CameraMovementRight)){
                        cameraComponent.Heading += 0.05f;
                        transform.Position = Vector3.Subtract(cameraComponent.Target, new Vector3((float)(cameraComponent.Distance * Math.Sin(cameraComponent.Heading + Math.PI * 0.5f)), cameraComponent.Height, (float)((-cameraComponent.Distance) * Math.Cos(cameraComponent.Heading + Math.PI * 0.5f))));

                    }

                    if (!Game1.Inst.Scene.EntityHasComponent<CBody>(input.Key)) {
                        continue;
                    }

                    if (_mapSystem != null)
                        body.Position.Y = _mapSystem.HeightPosition(body.Position.X, body.Position.Z);

                    if (currentState.IsKeyDown(inputValue.ForwardMovementKey))
                        body.Velocity.Z -= 5f;
                    if (currentState.IsKeyDown(inputValue.BackwardMovementKey))
                        body.Velocity.Z += 5f;
                    if (currentState.IsKeyDown(inputValue.LeftMovementKey))
                        body.Velocity.X -= 5f;
                    if (currentState.IsKeyDown(inputValue.RightMovementKey))
                        body.Velocity.X += 5f;

                    if (
                        !currentState.IsKeyDown(inputValue.ForwardMovementKey) &&
                        !currentState.IsKeyDown(inputValue.BackwardMovementKey) &&
                        !currentState.IsKeyDown(inputValue.LeftMovementKey) &&
                        !currentState.IsKeyDown(inputValue.RightMovementKey)
                    )
                    body.Velocity *= dt * 0.9f;

                    body.Velocity.X = Math.Max(body.Velocity.X, 10);
                    body.Velocity.Y = Math.Max(body.Velocity.Y, 10);                    
                    
                }
                //TempChat
               
                {
                    foreach (Keys key in pressedKeys)
                    {
                        if (!lastPressedKeys.Contains(key))
                        {    
                            Game1.Inst.RaiseInScene("KeyToType",key);
 
                        } 
                    }
                    //save the currently pressed keys so we can compare on the next update
                    lastPressedKeys = pressedKeys;
                }

                /*

                //((LookAtCamera)Camera).Target = new Vector3(m.M41, m.M42*0.0f, m.M43);
                //var ta = ((LookAtCamera)Camera).Target;
                var p = b.Position;
                var c = ((LookAtCamera)Camera).Position;
                var dist = 30f;
                var yDist = -20f;
                var h = b.Heading;

                // Vi positionerar kamera utifrån karaktärens heading (h), p = karaktärerns position, c = kamerans position, t = kamerans target, dist = avstånd till objektet
                // yDist = höjd för kameran, samt t = p -- alltså att kamerans target är position för karaktären.
                // Då gäller c=p-[d*sin(h + pi/2), y, (-d)*cos(h + pi/2)]

                c = Vector3.Subtract(p, new Vector3((float)(dist * Math.Sin(h + Math.PI * 0.5f)), yDist, (float)((-dist) * Math.Cos(h + Math.PI * 0.5f))));

                c.Y = -yDist; // Lock camera to given height
                p.Y = 0; // Target too because it was really ugly otherwise

                ((LookAtCamera)Camera).Target = p;
                ((LookAtCamera)Camera).Position = c;
                
                return Matrix.CreateLookAt(Position, (Vector3)m_Target, Up);

            */

            }
        }
    }
}
