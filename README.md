# FYS-X
Repository for a game made by Team FYS-X as the project during the thematic semester for Software Engineering.

_Made By_<br>
Tarik Yıldırım <br>
Stijn Klopper <br>
Rick den Otter <br>
Stan van Weringh <br>

## Best practices
Under this header, the best practices to ensure good teamwork will be explained.

### GIT/GitHub
The master branch is the main branch, the main code is kept up to date here. Changes to the master branch must be done through temporary branches starting with 'dev', as explained now:

#### Developing new functionality
New functionality will start on the dev branch. The project member will create a branch using the following naming scheme:
```
git checkout -b dev-{issue number}

Example for a pull request that changes the README.md:
git checkout -b dev-12

Example for a pull request that fixes a bug about chunks not generating:
git checkout -b dev-1

Example for a pull request that adds new functionality, adding biomes:
git checkout -b dev-2
```

After creating this branch and finishing the functionality, the project member will do the following to push the changes to the branch:
```
git add .

git commit -m {commit message}

(if branch is new)
git push --set-upstream origin {branch name}

(else)
git push
```

Then, the project member will go to GitHub, go to their branch in the branch selector and open a merge request.
The name of the pull request must look like:
```
{branchname}: {issue description}
```

To solve a connected issue, make sure that '{closes/fixes/resolves} #{issue number}' is in the comment, like:
```
Fixes #12
``` 

Afterwards, the other three project members will add their comments and approve of the merge request.