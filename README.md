# DoublePendulum

## A WPF 3D simulation to inspect the dynamics of a double pendulum

##### Note: the 'master' branch now contains version 2 of the application. If you are looking for the version which is described at https://www.codeproject.com/Articles/1098111/DoublePendulum-A-WPF-D-Simulation-to-Inspect-the-D then checkout branch 'version1'.

### Long time 3D Poincare view of many simulations at an energy of 0.8:
<img src="https://i.postimg.cc/7YyGN342/Double-Pendulum.jpg" style="width:880px;">

## If you just want to know how to work with the application, jump over the Physics part which is coming next. But to understand the application, you'll need to know at least a little bit about the physics.

### A Little Bit of Physics
A double pendulum consists of two pendulums attached end to end. They are allowed to move in the vertical plane only. We can think of a massive bob m1 suspended from a pivot so that it can swing freely in the vertical plane and a second bob m2 suspended from m1 in the same way. To make things simple both bobs shall have the same mass 'm' and the length of suspension is 'l' for both of them.

To initiate a motion of the pendulum, we can do several things:

1. displace m1 and/or m2 and then release them
2. briefly strike against m1 and/or m2
3. displace and strike against m1 and/or m2

Whatever we do, we put energy into the system. For case 1 it's potential energy, for case 2 kinetic energy and for case 3 it's both of them. To make things simple again, we think of a perfect pendulum without any friction or air resistance. That means that the pendulum will not lose any energy while it moves. Or in other words, once the pendulum is initiated by 1, 2 or 3 it will move forever.

### The Phase Space Trajectory

When initiating the pendulum in the above way, we give initial values to four essential properties:

1. the angle Q1 of mass 1 against the rest position
2. the angle Q2 of mass 2 against the rest position
3. the angular velocity W1 of mass 1 (called Omega1, ω1 or Ω1)
4. the angular velocity W2 of mass 2 (called Omega2, ω2 or Ω2)

When two pendulums are initiated with the same above four values, they will move in exactly the same way. There is no additional property which has an influence to the motion.

When the pendulum moves in our three dimensional space, m1 and m2 are constantly changing their angle and angular velocity. So for every point in time we can identify a point in a four dimensional space: (Q1, Q2, W1, W2). This space is called the phase space.

When moving in our 3D world, the pendulum is also moving along a path in its 4D phase space. This path is called the Phase Space Trajectory. Unfortunately we cannot take a look at this 4D path, but we can make projections into the 3D space, just like a 3D object like a tree is throwing a 2D shadow on the ground.

### Moving to Angular Momentum

Instead of dealing with angular velocity it makes a physicist's life easier to deal with angular momentum, which is directly related to the angular velocity. Angular momentum is the analogon to linear momentum which is the product of velocity and mass of an object.

So instead of W1 and W2 we will also deal with their corresponding angular momentums L1 and L2. The phase space coordinates then are (Q1, Q2, L1, L2).

### The Unit of Energy

We will scale the unit of energy, Joule, by a factor in such a way, that lifting a mass by the length of its suspension raises the energy by 1. So if both masses are at their rest position (Q1 = Q2 = 0) and have no angular velocity (W1 = W2 = 0) the total energy is 0.

Displacing m2 to an angle of 90° will lift m2 by the length of its suspension. Thus the energy is now 1. Moving further to 180° will end up in an energy of 2.

Displacing both masses to an angle of 90° leads to an energy of 3 and moving both angles to 180° ends up in an energy of 6. For sure only if W1 and W2 are 0.

### The Poincare Map
To visualise the long-term behaviour of the pendulum a 2D Poincare map showing Q1 and L1 is used. This map is the intersection of the 4D phase space at Q2 = 0: whenever the lower mass m2 swings through its rest position (Q2 = 0) from left to right (W2 > 0, we will add a point to the Poincare map at position Q1 and L1, i.e. the angle and angular momentum of m1.

On the other hand the Poincare map can also be used to specify initial values for a new simulation, i.e. values for Q1, Q2, W1 and W2. A certain point in the map directly gives us Q1 and L1. Q2 is 0 by definition of the map. L2 can be calculated due to the fact that the total energy is constant. L1 and L2 now leads to values for W1 and W2.

## The Double Pendulum Application
When you start the application for the first time, you will see a lot of empty space but hopefully also a 2D view of a double pendulum at its rest position. When you move the mouse over one of the yellow pendulum bobs they turn to red and you can 'grab' them by pressing the left mouse button. 

 After dragging one or both pendulum bobs to an initial position, you can click the [Start] button to start the simulation. The 2D pendulum view now shows the motion and every time the lower mass swings through its rest position from left to right, i.e. every time the Poincare condition is fulfilled, the bob turns to red for a short period and a new point is added to the 2D Poincare map to the left.

You can now start more simulations for the same energy by doing a right mouse click within the 2D Poincare map. If the mouse position (Q1, L1, Q2 = 0) leads to a valid L2, a new simulation is started immediately.

The 2D Poincare map has a small margin colored in dark gray. This margin can be used to specify maximum initial values for Q1 or L1. For example if you move the mouse into the margin to the right and do a right click there, Q1 will be set to the maximum Q1 value of the selected energy.

You can also zoom into the 2D Poincare map by dragging the mouse to bottom right. Dragging to the opposite direction will unzoom one level, i.e. show the previous area of the map. You can unzoom to the first level by dragging a long distance to the top left.

You can also specify the initial energy directly, without moving the pendulum bobs in the 2D pendulum view. Just enter the desired energy into the edit box in the upper right corner of the application and then click the [+] button next to it.

To limit accumulated errors in the calculation, simulations are stopped automatically after 3 minutes of simulated time. If you press [Start] again, the simulation will run for another 3 minutes.

You can change the speed of a simulation by doing a mouse click on the 'dT' field of the simulation. Left click doubles the speed, right click halves it.

The checkboxes 'H', 'M' and 'S' are used to highlight, mute or solo a simulation in the Poincare views (2D and 3D).

To delete a saved simulation, select the simulation and press the [Delete] key.