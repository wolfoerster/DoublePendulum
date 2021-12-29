# DoublePendulum
A WPF 3D simulation to inspect the dynamics of a double pendulum

Note: the 'master' branch now contains version 2 of the application.
If you are looking for the version which is described in the CodeProject page
https://www.codeproject.com/Articles/1098111/DoublePendulum-A-WPF-D-Simulation-to-Inspect-the-D
checkout branch 'version1'.

<img src="https://i.postimg.cc/7YyGN342/Double-Pendulum.jpg" style="width:880px;">

The double pendulum has four degrees of freedom: the angles Q1 and Q2 of masses 1 and 2 and the angular 
momentums L1 and L2. The angular momentums are related to the angular velocities W1 and W2 of masses 1 and 2.

To visualise the long-term behaviour of the pendulum a 2D Poincare map showing Q1 and L1 is used. 
The Poincare condition is: Q2 = 0 and W2 > 0. Since the total energy of the frictionless pendulum is constant, 
a point in the map specifies a unique point in the four dimensional phase space of the pendulum.

This WPF based Windows application has five views:

1. A 2D view of the pendulum to set up initial values for the simulation and showing the actual movement
2. A 2D Poincare map showing (Q1, L1) values of all simulations which can also be used to start a new simulation
3. A 3D Poincare map showing (Q1, L1, L2) values of all simulations
4. A 3D view of the pendulum showing the actual movement
5. A 3D projection of the pendulum's 4D phase space trajectory showing the actual movement

There are two ways to start a new simulation:

1. In the 2D pendulum view drag masses 1 and 2 to their start positions. If you select [Show Omega] you can also
specify their angular velocities by dragging the small circle in the center of each mass. The total energy E0 is displayed
at the top. Click [Start] to start the simulation.

2. In the 2D Poincare map do a right click at some position x, y. This point maps to some Q1 and L1. Since Q2 is 0 per 
definition of the Poincare map, the missing L2 can be calculated from the total energy E0. If that succeeds, the simulation 
will start immediately.

To delete a saved simulation, select the simulation and press the [Delete] key.