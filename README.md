# DoublePendulum
A WPF 3D simulation to inspect the dynamics of a double pendulum

<img src="http://xn--mariafrster-wfb.de/misc/DoublePendulum.jpg" style="width:880px;">

The double pendulum has four degrees of freedom: the angles Q1 and Q2 of masses 1 and 2 and the angular 
momentums L1 and L2. The angular momentums are related to the angular velocities W1 and W2 of masses 1 and 2.

To visualise the long-term behaviour of the pendulum a Poincare map showing Q1 and L1 is used. 
The Poincare condition is: Q2 = 0 and W2 > 0. Since the total energy of the frictionless pendulum is constant, 
a point in the map specifies a unique point in the four dimensional phase state of the pendulum.

This WPF based Windows application has three views:

1. A 3D view of the pendulum showing the actual movement
2. A 2D view of the pendulum to set up initial values for the simulation and showing the actual movement
3. A Poincare map showing (Q1, L1) values of the latest simulations which can also be used to start a new simulation


